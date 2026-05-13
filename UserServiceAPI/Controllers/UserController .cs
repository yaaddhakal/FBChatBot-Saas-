
using CoreCommon.Services;
using CoreCommon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserServiceAPI.DTOs.Users;
using UserServiceAPI.Interfaces;

namespace UserServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            ICurrentUserService currentUser,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        // ============================================================
        // GET ALL USERS
        // GET: api/User
        // ============================================================
        [HttpGet("GetAllUsersByCompanyIdAsync")]
        public async Task<IActionResult> GetAllUsersByCompanyIdAsync()
        {
            _logger.LogInformation("GetAllUsers by UserId: {UserId} CompanyId: {CompanyId}",
                _currentUser.UserId, _currentUser.CompanyId);

            var result = await _userRepository.GetAllUsersByCompanyIdAsync(_currentUser.CompanyId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================================
        // GET USER BY ID
        // GET: api/User/5
        // ============================================================
        [HttpGet("GetUserByIdAsync")]
        [Authorize(Roles = "Accountant")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            _logger.LogInformation("GetUserById: {UserId}", id);

            var result = await _userRepository.GetUserByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("GetAllUsersAsync")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            _logger.LogInformation("GetUserById: ");

            var result = await _userRepository.GetAllUsersAsync();
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("GetAllUsersDetailAsync")]
        public async Task<IActionResult> GetAllUsersDetailAsync()
        {
            _logger.LogInformation("GetUserById: ");

            var result = await _userRepository.GetAllUsersDetailsAsync();
            return result.Success ? Ok(result) : NotFound(result);
        }
        // ============================================================
        // GET USER WITH FULL DETAIL
        // GET: api/User/5/detail
        // ============================================================
        [HttpGet("GetUserWithDetailAsync")]
        public async Task<IActionResult> GetUserWithDetailAsync(int id)
        {
            _logger.LogInformation("GetUserWithDetail: {UserId}", id);

            var result = await _userRepository.GetUserWithDetailAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ============================================================
        // GET USERS BY ROLE
        // GET: api/User/role/2
        // ============================================================
        [HttpGet("GetUsersByRoleAndCompanyIdAsync")]
        public async Task<IActionResult> GetUsersByRoleAndCompanyIdAsync(int roleId)
        {
            _logger.LogInformation("GetUsersByRole: {RoleId} by UserId: {UserId}",
                roleId, _currentUser.UserId);

            var result = await _userRepository
                .GetUsersByRoleAndCompanyIdAsync(roleId, _currentUser.CompanyId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================================
        // CREATE USER
        // POST: api/User
        // ============================================================
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResultData<string>.Fail("Invalid request data"));

            _logger.LogInformation("CreateUser: {Username} by UserId: {UserId}",
                dto.Username, _currentUser.UserId);

            var result = await _userRepository
                .CreateUserAsync(dto, _currentUser.UserId);

            return result.Success
                ? CreatedAtAction(nameof(GetUserByIdAsync),
                    new { id = result.Data }, result)
                : BadRequest(result);
        }

        // ============================================================
        // UPDATE USER
        // PUT: api/User/5
        // ============================================================
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResultData<string>.Fail("Invalid request data"));

            if (id != dto.UserId)
                return BadRequest(ResultData<string>.Fail("User ID mismatch"));

            _logger.LogInformation("UpdateUser: {UserId} by UserId: {CurrentUserId}",
                id, _currentUser.UserId);

            var result = await _userRepository
                .UpdateUserAsync(dto, _currentUser.UserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================================
        // DELETE USER (Admin only)
        // DELETE: api/User/5
        // ============================================================
        [HttpDelete("DeleteUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("DeleteUser: {UserId} by Admin: {AdminId}",
                id, _currentUser.UserId);

            var result = await _userRepository.DeleteUserAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ============================================================
        // TOGGLE USER STATUS
        // PATCH: api/User/5/toggle
        // ============================================================
        [HttpPatch("ToggleUserStatus")]
        public async Task<IActionResult> ToggleUserStatus(int id, [FromBody] bool isActive)
        {
            _logger.LogInformation("ToggleUserStatus: {UserId} to {IsActive} by {CurrentUserId}",
                id, isActive, _currentUser.UserId);

            var result = await _userRepository.ToggleUserStatusAsync(id, isActive);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ============================================================
        // CHANGE PASSWORD
        // POST: api/User/change-password
        // ============================================================
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResultData<string>.Fail("Invalid request data"));

            // User can only change their own password
            if (dto.UserId != _currentUser.UserId)
                return Forbid();

            _logger.LogInformation("ChangePassword for UserId: {UserId}",
                _currentUser.UserId);

            var result = await _userRepository.ChangePasswordAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================================
        // RESET PASSWORD (Admin only)
        // POST: api/User/reset-password
        // ============================================================
        [HttpPost("reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResultData<string>.Fail("Invalid request data"));

            _logger.LogInformation("ResetPassword for UserId: {UserId} by Admin: {AdminId}",
                dto.UserId, _currentUser.UserId);

            var result = await _userRepository
                .ResetPasswordAsync(dto.UserId, dto.NewPassword, _currentUser.UserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================================
        // UPDATE KYC (Admin only)
        // PUT: api/User/kyc
        // ============================================================
        [HttpPut("kyc")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateKYC([FromBody] KYCUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResultData<string>.Fail("Invalid request data"));

            _logger.LogInformation("UpdateKYC for UserId: {UserId} by Admin: {AdminId}",
                dto.UserId, _currentUser.UserId);

            var result = await _userRepository
                .UpdateKYCAsync(dto, _currentUser.UserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================================
        // CHECK USERNAME EXISTS
        // GET: api/User/check-username/john
        // ============================================================
        [HttpGet("check-username/{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUsername(string username)
        {
            var result = await _userRepository.IsUsernameExistsAsync(username);
            return Ok(result);
        }

        // ============================================================
        // CHECK EMAIL EXISTS
        // GET: api/User/check-email/john@example.com
        // ============================================================
        [HttpGet("CheckEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var result = await _userRepository.IsEmailExistsAsync(email);
            return Ok(result);
        }
    }
}
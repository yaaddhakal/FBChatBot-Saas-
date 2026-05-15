using AuthAPI.Function.Services;
using AuthAPI.Interfaces;
using AuthAPI.Models.LoginRequest;
using CoreCommon.AuthModel.RefreshToken;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Security.Claims;
using CoreCommon.HelperCommon.Enums;
using System.IdentityModel.Tokens.Jwt;


namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly ILogInRepository _logInRepository;
       

    
        public AuthController(
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            JwtService jwtService,
            ILogger<AuthController> logger,
            ILogInRepository logInRepository
            )
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _jwtService = jwtService;
            _logger = logger;
            _logInRepository = logInRepository;
          
        }
       
        /// <summary>
        /// Login endpoint - Requires API Key, returns JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            var tokenResponse = await _logInRepository.login(request.Username, request.Password);

            if (!tokenResponse.Success || tokenResponse.Data == null)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return ActionResultHelper.FromResult(this, tokenResponse); 
            }
            return  ActionResultHelper.FromResult(this, tokenResponse);
        }
      
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(request.accessToken);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            int userId = int.Parse(userIdClaim);

            var tokenResponse = await _jwtService.AllTokenRefreshAsync(userId, request.RefreshToken);
            if(!tokenResponse.Success || tokenResponse.Data == null)
            {
                _logger.LogWarning("Failed token refresh attempt for user ID: {UserId}", userId);
                return ActionResultHelper.FromResult(this, tokenResponse);
            };
           
            return  ActionResultHelper.FromResult(this, tokenResponse);
        }

        //[HttpPost("logout")]
        //public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var userId = userIdClaim != null ? int.Parse(userIdClaim) : (int?)null;
        //    if (userId == null)
        //        return BadRequest(ResultData<object>.Fail("Invalid user token", ResultStatusCode.BadRequest));

        //    var result = await _logInRepository.Logout(request.RefreshToken, (int)userId);
        //    return ActionResultHelper.FromResult(this, result);
        //}

        //[HttpPost("LogoutAll")]
        //[Authorize]
        //public async Task<IActionResult> LogoutAll([FromBody] RefreshRequestDto request)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var userId = userIdClaim != null ? int.Parse(userIdClaim) : (int?)null;
        //    if (userId == null)
        //        return BadRequest(ResultData<object>.Fail("Invalid user token", ResultStatusCode.BadRequest));

        //    var result = await _logInRepository.LogoutAll(request.RefreshToken, (int)userId);
        //    return ActionResultHelper.FromResult(this, result);
        //}

        //[HttpGet("CurrentUserInfo")]
        //[Authorize(Roles = "Admin")]
        //public IActionResult GetCurrentUser()
        //{

        //        // Extract claims from JWT
        //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        //        var role = User.FindFirst("role")?.Value;
        //        var department = User.FindFirst("department")?.Value;
        //        var companyId = User.FindFirst("companyId")?.Value;

        //    var userInfo = new 
        //        {
        //            UserId = userId,
        //            Username = username,
        //            Role = role,
        //            Department = department,
        //            CompanyId = companyId
        //        };

        //    return Ok(ResultData<object>.Ok(userInfo));

        //}
    }
}

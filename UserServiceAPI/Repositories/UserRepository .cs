using BCrypt.Net;
using CoreCommon.DbService;
using System.Data;
using UserServiceAPI.DTOs;
using UserServiceAPI.DTOs.Users;
using UserServiceAPI.Interfaces;

namespace UserServiceAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbService _dbService;

        public UserRepository(IDbService dbService)
        {
            _dbService = dbService;
        }

        // ============================================================
        // CREATE USER
        // ============================================================
        public async Task<ResultData<int>> CreateUserAsync(UserCreateDto dto, int createdBy)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var result = await _dbService.GetAsync<SpResult>(
                "sp_ManageUser",
                new
                {
                    Flag = "CREATE",
                    dto.FullName,
                    dto.Username,
                    PasswordHash = passwordHash,
                    dto.RoleId,
                    dto.CompanyId,
                    dto.DOB,
                    dto.Gender,
                    dto.Email,
                    dto.MobileNo,
                    dto.IssuedDate_From,
                    dto.IssuedDate_To,
                    dto.ImageLocation,
                    dto.Country,
                    dto.State,
                    dto.City,
                    dto.Address,
                    dto.PostalCode,
                    dto.NationalId,
                    dto.Occupation,
                    dto.BloodGroup,
                    dto.AlternateMobile,
                    dto.EmergencyContact,
                    dto.AccountLimit,
                    ActionBy = createdBy
                },
                CommandType.StoredProcedure);

            return result.Data?.Result > 0
                ? ResultData<int>.Ok(result.Data.Result)
                : ResultData<int>.Fail(result.Data?.Message ?? "Failed to create user");
        }

        // ============================================================
        // UPDATE USER
        // ============================================================
        public async Task<ResultData<bool>> UpdateUserAsync(UserUpdateDto dto, int updatedBy)
        {
            var result = await _dbService.GetAsync<SpResult>(
                "sp_ManageUser",
                new
                {
                    Flag = "UPDATE",
                    dto.UserId,
                    dto.FullName,
                    dto.Email,
                    dto.MobileNo,
                    dto.RoleId,
                    dto.CompanyId,
                    dto.DOB,
                    dto.Gender,
                    dto.IssuedDate_From,
                    dto.IssuedDate_To,
                    dto.IsActive,
                    dto.ImageLocation,
                    dto.Country,
                    dto.State,
                    dto.City,
                    dto.Address,
                    dto.PostalCode,
                    dto.NationalId,
                    dto.Occupation,
                    dto.BloodGroup,
                    dto.AlternateMobile,
                    dto.EmergencyContact,
                    dto.AccountLimit,
                    dto.KYCStatus,
                    dto.KYCDocument,
                    ActionBy = updatedBy
                },
                CommandType.StoredProcedure);

            return result.Data?.Result > 0
                ? ResultData<bool>.Ok(true)
                : ResultData<bool>.Fail(result.Data?.Message ?? "Failed to update user");
        }

        // ============================================================
        // DELETE USER
        // ============================================================
        public async Task<ResultData<bool>> DeleteUserAsync(int userId)
        {
            var result = await _dbService.GetAsync<SpResult>(
                "sp_ManageUser",
                new
                {
                    Flag = "DELETE",
                    UserId = userId
                },
                CommandType.StoredProcedure);

            return result.Data?.Result > 0
                ? ResultData<bool>.Ok(true)
                : ResultData<bool>.Fail(result.Data?.Message ?? "User not found");
        }

        // ============================================================
        // TOGGLE USER STATUS
        // ============================================================
        public async Task<ResultData<bool>> ToggleUserStatusAsync(int userId, bool isActive)
        {
            var result = await _dbService.GetAsync<SpResult>(
                "sp_ManageUser",
                new
                {
                    Flag = "TOGGLE",
                    UserId = userId,
                    IsActive = isActive
                },
                CommandType.StoredProcedure);

            return result.Data?.Result > 0
                ? ResultData<bool>.Ok(true)
                : ResultData<bool>.Fail(result.Data?.Message ?? "User not found");
        }

        // ============================================================
        // GET USER BY ID
        // ============================================================
        public async Task<ResultData<UserResponseDto>> GetUserByIdAsync(int userId)
        {
            return await _dbService.GetAsync<UserResponseDto>(
                "sp_ManageUser",
                new
                {
                    Flag = "GETBYID",
                    UserId = userId
                },
                CommandType.StoredProcedure);
        }

        // ============================================================
        // GET USER WITH DETAIL
        // ============================================================
        public async Task<ResultData<UserDetailResponseDto>> GetUserWithDetailAsync(int userId)
        {
            return await _dbService.GetAsync<UserDetailResponseDto>(
                "sp_ManageUser",
                new
                {
                    Flag = "GETDETAILBYID",
                    UserId = userId
                },
                CommandType.StoredProcedure);
        }
        // ============================================================
        // GET ALL USERS
        // ============================================================
        public async Task<ResultData<List<UserListDto>>> GetAllUsersAsync()
        {
            return await _dbService.GetAllAsync<UserListDto>(
                "sp_ManageUser",
                new
                {
                    Flag = "GETAllUSERONLY"

                },
                CommandType.StoredProcedure);
        }

        // ============================================================
        // GET ALL USERSDetails
        // ============================================================
        public async Task<ResultData<List<UserDetailResponseDto>>> GetAllUsersDetailsAsync()
        {
            return await _dbService.GetAllAsync<UserDetailResponseDto>(
                "sp_ManageUser",
                new
                {
                    Flag = "GETDETAILAllUSERS"

                },
                CommandType.StoredProcedure);
        }
        // ============================================================
        // GET ALL USERS
        // ============================================================
        public async Task<ResultData<List<UserListDto>>> GetAllUsersByCompanyIdAsync(int companyId)
        {
            return await _dbService.GetAllAsync<UserListDto>(
                "sp_ManageUser",
                new
                {
                    Flag = "GETALLByCompanyID",
                    CompanyId = companyId
                },
                CommandType.StoredProcedure);
        }

        // ============================================================
        // GET USERS BY ROLE
        // ============================================================
        public async Task<ResultData<List<UserListDto>>> GetUsersByRoleAndCompanyIdAsync(int roleId, int companyId)
        {
            return await _dbService.GetAllAsync<UserListDto>(
                "sp_ManageUser",
                new
                {
                    Flag = "GETBYROLEAndCompanyId",
                    RoleId = roleId,
                    CompanyId = companyId
                },
                CommandType.StoredProcedure);
        }

        // ============================================================
        // CHANGE PASSWORD
        // ============================================================
        public async Task<ResultData<bool>> ChangePasswordAsync(ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return ResultData<bool>.Fail("Passwords do not match");

            // Get current hash
            var currentHash = await _dbService.GetScalarAsync<string>(
                "SELECT PasswordHash FROM tbl_Users WHERE UserId = @UserId",
                new { dto.UserId });

            if (string.IsNullOrEmpty(currentHash.Data))
                return ResultData<bool>.Fail("User not found");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, currentHash.Data))
                return ResultData<bool>.Fail("Current password is incorrect");

            // Hash new password
            var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            var result = await _dbService.GetAsync<SpResult>(
                "sp_ManageUser",
                new
                {
                    Flag = "CHANGEPASSWORD",
                    dto.UserId,
                    PasswordHash = newHash
                },
                CommandType.StoredProcedure);

            return result.Data?.Result > 0
                ? ResultData<bool>.Ok(true)
                : ResultData<bool>.Fail(result.Data?.Message ?? "Failed to change password");
        }

        // ============================================================
        // RESET PASSWORD
        // ============================================================
        public async Task<ResultData<bool>> ResetPasswordAsync(int userId, string newPassword, int updatedBy)
        {
            var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            var result = await _dbService.GetAsync<SpResult>(
                "sp_ManageUser",
                new
                {
                    Flag = "RESETPASSWORD",
                    UserId = userId,
                    PasswordHash = newHash,
                    ActionBy = updatedBy
                },
                CommandType.StoredProcedure);

            return result.Data?.Result > 0
                ? ResultData<bool>.Ok(true)
                : ResultData<bool>.Fail(result.Data?.Message ?? "Failed to reset password");
        }

        // ============================================================
        // UPDATE KYC
        // ============================================================
        public async Task<ResultData<bool>> UpdateKYCAsync(KYCUpdateDto dto, int updatedBy)
        {
            var result = await _dbService.GetAsync<SpResult>(
                "sp_ManageUser",
                new
                {
                    Flag = "KYC",
                    dto.UserId,
                    dto.KYCStatus,
                    dto.KYCDocument,
                    ActionBy = updatedBy
                },
                CommandType.StoredProcedure);

            return result.Data?.Result > 0
                ? ResultData<bool>.Ok(true)
                : ResultData<bool>.Fail(result.Data?.Message ?? "Failed to update KYC");
        }

        // ============================================================
        // CHECK USERNAME EXISTS
        // ============================================================
        public async Task<ResultData<bool>> IsUsernameExistsAsync(string username)
        {
            var result = await _dbService.GetScalarAsync<int>(
                "sp_ManageUser",
                new
                {
                    Flag = "CHECKUSERNAME",
                    Username = username
                },
                CommandType.StoredProcedure);
            if (result.Success==false || result.Data <= 0)
            {
                return ResultData<bool>.Fail(result.Error ?? "Failed to check username");
            }

            return ResultData<bool>.Ok(result.Data > 0);
        }

        // ============================================================
        // CHECK EMAIL EXISTS
        // ============================================================
        public async Task<ResultData<bool>> IsEmailExistsAsync(string email)
        {
            var result = await _dbService.GetScalarAsync<int>(
                "sp_ManageUser",
                new
                {
                    Flag = "CHECKEMAIL",
                    Email = email
                },
                CommandType.StoredProcedure);

            if (result.Success==false || result.Data <= 0)
            {
                return ResultData<bool>.Fail(result.Error ?? "Failed to check username");
            }

            return ResultData<bool>.Ok(result.Data > 0);
        }
    }
}

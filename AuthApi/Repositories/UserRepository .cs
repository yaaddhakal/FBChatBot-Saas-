
using AuthAPI.Interfaces;
using AuthAPI.Repositories;
using CoreCommon.DbService;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using AuthAPI.Models.Entites.User;
using System.Data;

namespace AuthAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger; // Fixed: was ILogger<TokenRepository>
        private readonly IDbService _dbService;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger, IDbService dbservice)
        {
            _configuration = configuration;
            _logger = logger;
            _dbService = dbservice;
        }

        public async Task<ResultData<UserViewModel?>> ValidateUserAsync(string username, string password)
        {
            const string sql = @"
        SELECT u.*, r.RoleName, d.DepartmentName, c.CompanyName,r.DepartmentId
        FROM dbo.tbl_Users u
        INNER JOIN dbo.tbl_Company c ON u.CompanyId = c.CompanyId
        INNER JOIN dbo.tbl_Roles r ON u.RoleId = r.RoleId
        INNER JOIN dbo.tbl_Department d ON r.DepartmentId = d.DepartmentId
        WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash";

            var parameters = new
            {
                Username = username,
                PasswordHash = password // TODO: hash the password before comparing e.g. BCrypt.HashPassword(password)
            };

            var result = await _dbService.GetAsync<UserViewModel>(sql, parameters, CommandType.Text);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("User validation failed for Username: {Username}", username);
                var rResult=ResultData<UserViewModel?>.Fail(string.IsNullOrWhiteSpace( result.Error) ? "Invalid username or password":result.Error, ResultStatusCode.BadRequest);
                var qq = ResultData<UserViewModel?>.Fail("Invalid username or password", ResultStatusCode.BadRequest);
              
                return rResult;
            }

            _logger.LogInformation("User validated successfully: {Username}", username);
            return ResultData<UserViewModel?>.Ok(result.Data, ResultStatusCode.Ok);
        }

        public async Task<ResultData<UserViewModel?>> GetUserByIdAsync(int userId)
        {
            const string sql = @"
        SELECT *
        FROM dbo.tbl_Users
        WHERE UserId = @UserId";

            var result = await _dbService.GetAsync<UserViewModel>(sql, new { UserId = userId }, CommandType.Text);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("No user found with UserId: {UserId}", userId);
                return ResultData<UserViewModel?>.Fail(result.Error ?? "User not found", ResultStatusCode.NotFound);
            }

            _logger.LogInformation("User retrieved successfully: {UserId}", userId);
            return ResultData<UserViewModel?>.Ok(result.Data, ResultStatusCode.Ok);
        }
    }
}

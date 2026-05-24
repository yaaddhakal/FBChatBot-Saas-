
using AuthAPI.Interfaces;
using AuthAPI.Models.Entites.User;
using AuthAPI.Repositories;
using CoreCommon.DbService;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using CoreCommon.Models.UsersModels;
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

        public async Task<ResultData<UserViewDto?>> ValidateUserAsync(string username, string password)
        {
            const string sql = @"sp_UserLogin";
            var pass= PasswordEncryption.HashPassword(password);
            var parameters = new
            {
                Login = username,
                PasswordHash = pass // TODO: hash the password before comparing e.g. BCrypt.HashPassword(password)
            };

            var result = await _dbService.GetAsync<UserViewDto>(sql, parameters, CommandType.StoredProcedure);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("User validation failed for Username: {Username}", username);
                var rResult=ResultData<UserViewDto?>.Fail(string.IsNullOrWhiteSpace( result.Error) ? "Invalid username or password":result.Error, ResultStatusCode.BadRequest);
                var qq = ResultData<UserViewDto?>.Fail("Invalid username or password", ResultStatusCode.BadRequest);
              
                return rResult;
            }

            _logger.LogInformation("User validated successfully: {Username}", username);
            return ResultData<UserViewDto?>.Ok(result.Data, ResultStatusCode.Ok);
        }

        public async Task<ResultData<UserViewDto?>> GetUserByIdAsync(int userId)
        {
            const string sql = @"sp_Users_CRUD";

            var result = await _dbService.GetAsync<UserViewDto>(sql, 
                new 
                {
                    Action = "SELECTALLBYID",
                UserID = userId
                }, 
                CommandType.StoredProcedure);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("No user found with UserId: {UserId}", userId);
                return ResultData<UserViewDto?>.Fail(result.Error ?? "User not found", ResultStatusCode.NotFound);
            }

            _logger.LogInformation("User retrieved successfully: {UserId}", userId);
            return ResultData<UserViewDto?>.Ok(result.Data, ResultStatusCode.Ok);
        }
    }
}

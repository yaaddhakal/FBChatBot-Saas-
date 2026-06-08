
using AuthAPI.Interfaces;
using AuthAPI.Models.Entites.User;
using AuthAPI.Repositories;
using CoreCommon.DbService;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using CoreCommon.Models.UsersModels;
using Dapper;
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
        public async Task<ResultData<UserModelDto?>> GetSingleUserByIdAsync(int userId)
        {
            const string sql = @"sp_Users_CRUD";

            var result = await _dbService.GetAsync<UserModelDto>(sql,
                new
                {
                    Action = "SELECT",
                    UserID = userId
                },
                CommandType.StoredProcedure);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("No user found with UserId: {UserId}", userId);
                return ResultData<UserModelDto?>.Fail(result.Error ?? "User not found", ResultStatusCode.NotFound);
            }

            _logger.LogInformation("User retrieved successfully: {UserId}", userId);
            return ResultData<UserModelDto?>.Ok(result.Data, ResultStatusCode.Ok);
        }
        public async Task<ResultData<SqlMapper.GridReader>> ValidateUserAsync(string username, string password)
        {
            const string sql = @"sp_UserLogin";
            var pass= PasswordEncryption.HashPassword(password);
            var parameter = new DynamicParameters();

            parameter.Add("@Login", username);
            parameter.Add("@PasswordHash", pass);
            parameter.Add("@UserID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("@ResultMessage", dbType: DbType.String, direction: ParameterDirection.Output);
            var parameters = new
            {
                Login = username,
                PasswordHash = pass // TODO: hash the password before comparing e.g. BCrypt.HashPassword(password)
             
            };

            var result = await _dbService.GetQueryMultipleAsync(sql, parameters, CommandType.StoredProcedure);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("User validation failed for Username: {Username}", username);
                return ResultData < SqlMapper.GridReader >.Fail("Invalid username or password", ResultStatusCode.BadRequest);
            }
            //var user = await result.Data.ReadFirstOrDefaultAsync<UserDto>();
            //var aa = result.Data.ReadFirstOrDefault<string>();
            _logger.LogInformation("User validated successfully: {Username}", username);
            return  ResultData<SqlMapper.GridReader>.Ok(result.Data, ResultStatusCode.Ok);
        }

        public async Task<ResultData<UserDto?>> GetUserByIdAsync(int userId)
        {
            const string sql = @"sp_Users_CRUD";

            var result = await _dbService.GetAsync<UserDto>(sql, 
                new 
                {
                    Action = "SELECTALLBYID",
                UserID = userId
                }, 
                CommandType.StoredProcedure);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("No user found with UserId: {UserId}", userId);
                return ResultData<UserDto?>.Fail(result.Error ?? "User not found", ResultStatusCode.NotFound);
            }

            _logger.LogInformation("User retrieved successfully: {UserId}", userId);
            return ResultData<UserDto?>.Ok(result.Data, ResultStatusCode.Ok);
        }

       
    }
}

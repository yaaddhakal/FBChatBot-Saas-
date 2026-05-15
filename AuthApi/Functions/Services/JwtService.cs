


using AuthAPI.Interfaces;
using AuthAPI.Models.Entites.User;
using AuthAPI.Repositories;
using CoreCommon.AuthModel.RefreshToken;
using CoreCommon.DbService;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using CoreCommon.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace AuthAPI.Function.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<JwtService> _logger;
        private readonly IDbService _dbService;
        private readonly ITokenRepository _tokenRepository;
        public JwtService(IUserRepository userRepository, IConfiguration config, IDbService dbService, ILogger<JwtService>  logger, ITokenRepository tokenRepository)
        {
            _config = config;
           _dbService= dbService;
            _logger = logger;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
        }

        public string GenerateJWTToken(UsersViewModel user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim("role", user.RoleName),
            new Claim("roleId", user.RoleID.ToString()),
            new Claim("IndustryID", user.IndustryID.ToString()),
            new Claim("IndustryName", user.IndustryName),
            new Claim("TenantName", user.TenantName),
            new Claim("TenantID", user.TenantID.ToString()),
            new Claim("UserType", user.UserType)
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ResultData<string>> IssueNewRefreshTokenAsync(int userId, string refreshToken, bool isSingleInactive = true)
        {
            var newToken = GenerateRefreshToken();
            var expireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"] ?? "7");
            var expiresAt = DateTime.UtcNow.AddDays(expireDays);
           ;


            var commands = new List<(string, object?, CommandType)>();

                if (isSingleInactive)
                {
                    // Revoke only the specific token
                    const string revokeSql = @"
                    UPDATE tbl_RefreshTokens
                    SET IsRevoked = 1, RevokedAt = @RevokedAt
                    WHERE UserId = @UserId AND Token = @Token AND IsRevoked = 0";

                    commands.Add((revokeSql, new
                    {
                        UserId = userId,
                        Token = refreshToken,
                        RevokedAt = DateTime.UtcNow
                    }, CommandType.Text));
                }
                else
                {
                    // Revoke all active tokens for this user
                    const string revokeAllSql = @"
                    UPDATE tbl_RefreshTokens
                    SET IsRevoked = 1, RevokedAt = @RevokedAt
                    WHERE UserId = @UserId AND IsRevoked = 0";

                    commands.Add((revokeAllSql, new
                    {
                        UserId = userId,
                        RevokedAt = DateTime.UtcNow
                    }, CommandType.Text));
                }

                // Insert new token
                const string insertSql = @"
                INSERT INTO tbl_RefreshTokens (UserId, Token, ExpiresAt, CreatedAt, IsRevoked)
                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, 0)";

                commands.Add((insertSql, new
                {
                    UserId = userId,
                    Token = newToken,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow
                }, CommandType.Text));

                // Execute all commands atomically
                var result = await _dbService.ExecuteTransactionAsync(commands);

                if (result.Success)
                {
                    _logger.LogInformation("Issued new refresh token for UserId: {UserId}, Expires: {ExpiresAt}", userId, expiresAt);
                    return ResultData<string>.Ok(newToken, ResultStatusCode.Ok);
                }

                return ResultData<string>.Fail( "Failed to issue refresh token.", ResultStatusCode.BadRequest);
        }
        public async Task<ResultData<UsersViewModel?>> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var expireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"] ?? "7");
            var expiresAt = DateTime.UtcNow.AddDays(expireDays);

            // Invalidate all existing tokens for this user (pass null token, isForSingle=false)
            // await InvalidateUserTokensAsync(userId, null, false);

            var sql = @"sp_UserTokens_CRUD";

            var result = await _dbService.GetScalarAsync<int>(
                sql,
                new
                {
                    Action = "REFRESH",
                    UserID = userId,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt
                },
                commandType: CommandType.StoredProcedure
            );

            var isValid = result.Success && result.Data > 0;

            if (!isValid)
            {
                _logger.LogWarning("Invalid refresh token validation attempt for UserId: {UserId}", userId);

                return ResultData<UsersViewModel?>.Fail(result.Error ?? "Invalid refresh token", ResultStatusCode.NotFound);
            }

            var mresult= await _userRepository.GetUserByIdAsync(userId);
            if(!mresult.Success  || mresult.Data == null)
            {
                return ResultData<UsersViewModel?>.Fail("User not found", ResultStatusCode.NotFound);
            };
            return ResultData<UsersViewModel?>.Ok(mresult.Data, ResultStatusCode.Ok);
        }
        public async Task<ResultData<TokenResponseModel?>> AllTokenRefreshAsync(int userId, string refreshToken)
        {
            var expireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"] ?? "7");
            var expiresAt = DateTime.UtcNow.AddDays(expireDays);

            var isValidateData = await ValidateRefreshTokenAsync(userId, refreshToken);
            if (!isValidateData.Success || isValidateData.Data == null)
            {
                return ResultData<TokenResponseModel?>.Fail(isValidateData.Error ?? "Invalid refresh token", ResultStatusCode.NotFound);
            }

            var user = isValidateData.Data;
            var jwtToken = GenerateJWTToken(user);
             refreshToken = GenerateRefreshToken();

            var saveResult = await _tokenRepository.SaveRefreshTokenAsync(user.UserID, refreshToken);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save refresh token for user {UserId}: {Error}", user.UserID, saveResult.Error);
                return ResultData<TokenResponseModel>.Fail("Failed to save refresh token.");
            }

            //_logger.LogInformation("Successful login for user: {Username} (UserId: {UserId})", user.UserName, user.UserID);

            return ResultData<TokenResponseModel?>.Ok(new TokenResponseModel
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                UserId = user.UserID,
                Username = user.UserName,
                UserType = user.UserType,
                RoleName = user.RoleName,
                ExpiresIn = int.Parse(_config["Jwt:ExpireMinutes"]) ///1800
            });
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
      
    }
}

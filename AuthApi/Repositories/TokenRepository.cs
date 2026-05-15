
using AuthAPI.Interfaces;
using CoreCommon.DbService;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using CoreCommon.AuthModel.RefreshToken;
using System.Data;

namespace AuthAPI.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenRepository> _logger;
        private readonly IDbService _dbService;

        public TokenRepository(IConfiguration configuration, ILogger<TokenRepository> logger, IDbService dbservice)
        {
            _configuration = configuration;
            _logger = logger;
            _dbService = dbservice;
        }

        public async Task<ResultData<RefreshTokenModel>> GetOldRefreshTokenInfoAsync(string token)
        {
            var result = await _dbService.GetAsync<RefreshTokenModel>(
                @"SELECT TokenId, UserId, Token, ExpiresAt, CreatedAt, IsRevoked, RevokedAt
                  FROM tbl_RefreshTokens
                  WHERE Token = @Token",
                new { Token = token }
            );
            return result;
        }

        public async Task<ResultData<int>> SaveRefreshTokenAsync(int userId, string refreshToken)
        {
            var expireDays = int.Parse(_configuration["Jwt:RefreshTokenExpireDays"] ?? "7");
            var expiresAt = DateTime.UtcNow.AddDays(expireDays);

            // Invalidate all existing tokens for this user (pass null token, isForSingle=false)
           // await InvalidateUserTokensAsync(userId, null, false);

            var sql = @"sp_UserTokens_CRUD";

            var result = await _dbService.ExecuteAsync(
                sql,
                new
                {
                    Action = "LOGIN",
                    UserID = userId,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt
                },
                commandType: CommandType.StoredProcedure
            );

            if (result.Success && result.Data > 0)
            {
                _logger.LogInformation("Refresh token saved for UserId: {UserId}, Expires: {ExpiresAt}", userId, expiresAt);
            }

            return result;
        }

        public async Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var sql = @"
                SELECT COUNT(1) 
                FROM tbl_RefreshTokens 
                WHERE UserId = @UserId 
                    AND Token = @Token 
                    AND ExpiresAt > @Now 
                    AND IsRevoked = 0";

            var result = await _dbService.GetScalarAsync<int>(
                sql,
                new { UserId = userId, Token = refreshToken, Now = DateTime.UtcNow }
            );

            var isValid = result.Success && result.Data > 0;

            if (!isValid)
                _logger.LogWarning("Invalid refresh token validation attempt for UserId: {UserId}", userId);

            return isValid;
        }

        public async Task<ResultData<int>> InvalidateUserTokensAsync(int userId, string? token = null, bool isForSingle = false)
        {
            string sql = @"
                UPDATE tbl_RefreshTokens 
                SET IsRevoked = 1, RevokedAt = @RevokedAt 
                WHERE UserId = @UserId AND IsRevoked = 0";

            if (isForSingle)
            {
                if (string.IsNullOrEmpty(token))
                    throw new ArgumentException("Token must be provided when revoking a single device.");

                sql = @"
                    UPDATE tbl_RefreshTokens 
                    SET IsRevoked = 1, RevokedAt = @RevokedAt 
                    WHERE UserId = @UserId 
                      AND Token = @Token 
                      AND IsRevoked = 0";
            }

            var result = await _dbService.ExecuteAsync(
                sql,
                new { UserId = userId, Token = token, RevokedAt = DateTime.UtcNow },
                commandType: CommandType.Text
            );

            if (result.Success && result.Data > 0)
                _logger.LogInformation("Invalidated {Count} refresh tokens for UserId: {UserId}", result.Data, userId);
            else if (result.Success && result.Data == 0)
                _logger.LogWarning("No active refresh tokens found for UserId: {UserId}", userId);

            return result;
        }

        public async Task<ResultData<int>> CleanupExpiredTokensAsync(int userId)
        {
            var sql = @"
                DELETE FROM tbl_RefreshTokens 
                WHERE (ExpiresAt < @Now OR IsRevoked = 1)
                  AND UserId = @UserId";

            var result = await _dbService.ExecuteAsync(
                sql,
                new { UserId = userId, Now = DateTime.UtcNow.AddDays(-30) },
                commandType: CommandType.Text
            );

            if (result.Success && result.Data > 0)
                _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens for UserId: {UserId}", result.Data, userId);
            else if (result.Success && result.Data == 0)
                _logger.LogInformation("No expired/revoked refresh tokens found for UserId: {UserId}", userId);

            return result;
        }
    }
}

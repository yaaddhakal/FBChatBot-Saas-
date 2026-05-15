using AuthAPI.Function.Services;
using AuthAPI.Interfaces;
using Azure.Core;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using Microsoft.AspNetCore.Mvc;
using CoreCommon.AuthModel.RefreshToken;
using System.Security.Claims;

namespace AuthAPI.Repositories
{
    public class LogInRepository : ILogInRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LogInRepository> _logger;
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly ITokenRepository _tokenRepository;

        public LogInRepository(IConfiguration config ,ILogger<LogInRepository> logger,
            IUserRepository userRepository, JwtService jwtService, ITokenRepository tokenRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _tokenRepository = tokenRepository;
            _config = config;
        }

        public async Task<ResultData<TokenResponseModel>> login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return ResultData<TokenResponseModel>.Fail("Username and password cannot be empty.", ResultStatusCode.BadRequest);

            var userResult = await _userRepository.ValidateUserAsync(userName, password);
            if (!userResult.Success || userResult.Data == null)
                return ResultData<TokenResponseModel>.Fail(userResult.Error, ResultStatusCode.NotFound);

            var user = userResult.Data;
            if (!user.IsActive)
                return ResultData<TokenResponseModel>.Fail("User account is inactive.", ResultStatusCode.Unauthorized);

            var jwtToken = _jwtService.GenerateJWTToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var saveResult = await _tokenRepository.SaveRefreshTokenAsync(user.UserID, refreshToken);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save refresh token for user {UserId}: {Error}", user.UserID, saveResult.Error);
                return ResultData<TokenResponseModel>.Fail("Failed to save refresh token.");
            }

            //_logger.LogInformation("Successful login for user: {Username} (UserId: {UserId})", user.UserName, user.UserID);

            return ResultData<TokenResponseModel>.Ok(new TokenResponseModel
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

        //public async Task<ResultData<TokenResponseModel>> GetRefreshToken(RefreshRequestDto request,int userId)
        //{
        //    if (string.IsNullOrEmpty(request.RefreshToken))
        //        return ResultData<TokenResponseModel>.Fail("Invalid refresh token. Empty", ResultStatusCode.Unauthorized);

        //    string sql = "sp_UserTokens_CRUD";
        //    var parma = new { 
        //        Action="REFRESH",
        //        UserID = userId,
        //        RefreshToken=request.RefreshToken
        //    };
        //    var newToken = _jwtService.GenerateRefreshToken();
        //    var expireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"] ?? "7");
        //    var expiresAt = DateTime.UtcNow.AddDays(expireDays); 
        //    var newJwtToken = _jwtService.GenerateJWTToken(user);
        //    await _tokenRepository.CleanupExpiredTokensAsync(user.UserId);


        //    var result = new TokenResponseModel
        //    {
        //        Token = newJwtToken,
        //        RefreshToken = newrefreshToken.Data,
        //        UserId = user.UserId,
        //        Username = user.Username,
        //        RoleName = user.RoleName,
        //        ExpiresIn = int.Parse(_config["Jwt:ExpireMinutes"])///1800
        //    };
        //    return ResultData<TokenResponseModel>.Ok(result);
        //}

        //public async Task<ResultData<string>> Logout( string token,int userId)
        //{
        //    if (string.IsNullOrEmpty(token))
        //        return ResultData<string>.Fail("Invalid refresh token. Empty", ResultStatusCode.Unauthorized);
        //   var revokeResult = await _tokenRepository.InvalidateUserTokensAsync(userId, token,true);

        //    if (!revokeResult.Success)
        //    {
        //        _logger.LogError("Failed to revoke refresh token for UserId: {UserId}. {Error}", userId, revokeResult.Error);
        //        return ResultData<string>.Fail("Logout failed: Failed to revoke refresh token", ResultStatusCode.InternalServerError);
        //    }

        //    return ResultData<string>.Ok("Logout successful.");
        //}

        //public async Task<ResultData<string>> LogoutAll(string token, int userId)
        //{
        //    if (string.IsNullOrEmpty(token))
        //        return ResultData<string>.Fail("Invalid refresh token. Empty", ResultStatusCode.Unauthorized);
        //    var revokeResult = await _tokenRepository.InvalidateUserTokensAsync(userId, token, false);

        //    if (!revokeResult.Success)
        //    {
        //        _logger.LogError("Failed to revoke refresh token for UserId: {UserId}. {Error}", userId, revokeResult.Error);
        //        return ResultData<string>.Fail("Logout failed: Failed to revoke refresh token", ResultStatusCode.InternalServerError);
        //    }

        //    return ResultData<string>.Ok("Logout successful from all device.");
        //}
    }
}

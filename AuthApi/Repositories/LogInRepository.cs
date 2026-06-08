using AuthAPI.Function.Services;
using AuthAPI.Interfaces;
using AuthAPI.Models.Entites.User;
using Azure.Core;
using CoreCommon.AuthModel.RefreshToken;
using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using CoreCommon.Models.UsersModels;
using CoreCommon.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IOtpRepositories _otpRepository;
        private readonly IEmailService _emailService;
        public LogInRepository(IConfiguration config, ILogger<LogInRepository> logger,
            IUserRepository userRepository, JwtService jwtService, ITokenRepository tokenRepository,
            IOtpRepositories _otpRepositories)
        {
            _logger = logger;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _tokenRepository = tokenRepository;
            _config = config;
            _otpRepository = _otpRepositories;
        }

        public async Task<ResultData<TokenResponseModel>> login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return ResultData<TokenResponseModel>.Fail("Username and password cannot be empty.", ResultStatusCode.BadRequest);

            var userResult = await _userRepository.ValidateUserAsync(userName, password);
            if (!userResult.Success || userResult.Data == null)
                return ResultData<TokenResponseModel>.Fail(userResult.Error, ResultStatusCode.NotFound);

            UserDto user = new UserDto();
            string mstatus = string.Empty;

            using (var multi = userResult.Data)
            {
                user = await multi.ReadFirstOrDefaultAsync<UserDto>();
                mstatus = await multi.ReadFirstOrDefaultAsync<string>();
            }

            // ✅ handle error cases first
            switch (mstatus)
            {
                case "ERR_INACTIVE":
                    return ResultData<TokenResponseModel>.Fail("User account is inactive.", ResultStatusCode.Unauthorized);

                case "ERR_INVALID":
                    return ResultData<TokenResponseModel>.Fail("Invalid credentials.", ResultStatusCode.Unauthorized);

                case "ERR_EMAIL_UNVERIFIED":
                    // ✅ resend OTP and RETURN — don't fall through!
                    var otpResult = await _otpRepository.ResendOtpAsync(user.UserID);

                    return ResultData<TokenResponseModel>.Ok(new TokenResponseModel
                    {
                        UserId = user.UserID,
                        Username = user.UserName,
                        IsOtpSent = otpResult.Success,   // ✅ true or false
                        Status = "ERR_EMAIL_UNVERIFIED" // ✅ Angular opens OTP modal
                    });
            }

            // ✅ only reaches here if mstatus == "Success"
            var jwtToken = _jwtService.GenerateJWTToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var saveResult = await _tokenRepository.SaveRefreshTokenAsync(user.UserID, refreshToken);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save refresh token for user {UserId}: {Error}", user.UserID, saveResult.Error);
                return ResultData<TokenResponseModel>.Fail("Failed to save refresh token.");
            }

            return ResultData<TokenResponseModel>.Ok(new TokenResponseModel
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                UserId = user.UserID,
                Username = user.UserName,
                UserType = user.UserType,
                RoleName = user.RoleName,
                ExpiresIn = int.Parse(_config["Jwt:ExpireMinutes"]),
                IsOtpSent = false,
                Status = "Success"
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

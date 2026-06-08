using AuthAPI.Interfaces;
using AuthAPI.Models.Entites.User;
using AuthAPI.Models.Otp;
using CoreCommon.DbService;

using CoreCommon.Services.Interfaces;
using Dapper;
using System.Data;

namespace AuthAPI.Repositories
{
    public class OtpRepositories: IOtpRepositories
    {
        private readonly IDbService _dbService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public OtpRepositories(IDbService dbService, IEmailService emailService, IUserRepository userRepository)
        {
            _dbService = dbService;
            _emailService = emailService;
            _userRepository = userRepository;
        }
        public async Task<ResultData<string>> ResendOtpAsync(int userID)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserID", userID);
            parameters.Add("@ResultMessage",
                dbType: DbType.String,
                size: 100,
                direction: ParameterDirection.Output);

            var result = await _dbService.ExecuteSpWithOutputAsync(parameters, "sp_ResendOtp");

            if (!result.Success)
                return ResultData<string>.Fail(result.Error);

            // ✅ fetch new OTP and user then send email
            var otp = await GetLatestOtpAsync(userID);
            var user = await _userRepository.GetUserByIdAsync(userID);

            if (!otp.Success || !user.Success)
                return ResultData<string>.Fail("Failed to retrieve OTP or user information");

            var emailResult = await _emailService.SendOtpEmailAsync(
                user.Data.Email,
                user.Data.UserName,
                otp.Data.OtpCode
            );
            if (!emailResult.Success)
                return ResultData<string>.Fail("Failed to resend OTP email");

            return ResultData<string>.Ok("OTP resent successfully");
        }

        public async Task<ResultData<OtpDto>> GetLatestOtpAsync(int userID)
        {
            var query = @"
        SELECT TOP 1 OtpID, UserId, OtpCode, CreatedAt, ExpiresAt, IsUsed
        FROM UserOtps
        WHERE UserId = @UserID
        ORDER BY CreatedAt DESC";

            return await _dbService.GetAsync<OtpDto>(query, new { UserID = userID });
        }

        public async Task<ResultData<UserModelDto>> VerifyOtpAsync(VerifyOtpRequestDto re)
        {
            if (re.UserID <= 0)
                return ResultData<UserModelDto>.Fail("Invalid UserID");

            if (string.IsNullOrWhiteSpace(re.OtpCode) || re.OtpCode.Length != 6)
                return ResultData<UserModelDto>.Fail("Invalid OTP code");

            var parameters = new DynamicParameters();
            parameters.Add("@UserID", re.UserID);
            parameters.Add("@OtpCode", re.OtpCode);
            parameters.Add("@ResultMessage",
                dbType: DbType.String,
                size: 100,
                direction: ParameterDirection.Output);
            string sp = "sp_VerifyOtp";

            var result = await _dbService.VerifyOtpAsync(parameters, sp);
            if (!result.Success)
                return ResultData<UserModelDto>.Fail("User not found");
            // fetch full user after verification
            var user = await _userRepository.GetSingleUserByIdAsync(re.UserID);
            return ResultData<UserModelDto>.Ok(user.Data);


        }
    }
}

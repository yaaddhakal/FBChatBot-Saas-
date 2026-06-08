using AuthAPI.Models.Otp;

namespace AuthAPI.Interfaces
{
    public interface IOtpRepositories
    {
         Task<ResultData<string>> ResendOtpAsync(int userID);
        Task<ResultData<OtpDto>> GetLatestOtpAsync(int userID);
    }
}

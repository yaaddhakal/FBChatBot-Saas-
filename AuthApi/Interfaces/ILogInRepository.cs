using Microsoft.AspNetCore.Mvc;
using CoreCommon.AuthModel.RefreshToken;

namespace AuthAPI.Interfaces
{
    public interface ILogInRepository
    {
        Task<ResultData<TokenResponseModel>> login(string userName, string password);
        Task<ResultData<TokenResponseModel>> GetRefreshToken(RefreshRequestDto request);
        Task<ResultData<string>> Logout(string token, int userId);
        Task<ResultData<string>> LogoutAll(string token, int userId);
    }
}

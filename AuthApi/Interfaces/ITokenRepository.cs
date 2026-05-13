
using CoreCommon.AuthModel.RefreshToken;
using CoreCommon.HelperCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace AuthAPI.Interfaces
{
    /// <summary>
    /// Repository interface for managing refresh tokens
    /// </summary>
    public interface ITokenRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ResultData<RefreshTokenModel>> GetOldRefreshTokenInfoAsync(string token);
        Task<ResultData<int>> SaveRefreshTokenAsync(int userId, string refreshToken);
        Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
        Task<ResultData<int>> InvalidateUserTokensAsync(int userId, string? token = null, bool isForSingle = false);
        Task<ResultData<int>> CleanupExpiredTokensAsync(int userId);
     }
}


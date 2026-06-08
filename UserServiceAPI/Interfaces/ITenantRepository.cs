using CoreCommon.DbService;
using CoreCommon.Models.UsersModels;
using UserServiceAPI.DTOs.Tenant;

namespace UserServiceAPI.Interfaces
{
    public interface ITenantRepository
    {
        Task<ResultData<string>> ResendOtpAsync(int userID);
        Task<ResultData<List<IndustryDto>>> GetIndustriesByTenantAsync(int tenantID);
        Task<ResultData<List<TenantDto>>> GetAllTenantsListAsync();
        Task<ResultData<SignupResultDto>> SignupTenantAsync(SignupTenantRequestDto re);
        Task<ResultData<UserDto>> VerifyOtpAsync(VerifyOtpRequestDto re);
        Task<ResultData<TenantOtpDto>> GetLatestOtpAsync(int userID);
        Task<ResultData<List<TenantDto>>> GetAllTenantsAsync();
        Task<ResultData<TenantDto>> GetTenantByIdAsync(int tenantId);
        Task<ResultData<int>> CreateTenantAsync(TenantDto dto);
        Task<ResultData<bool>> UpdateTenantAsync(TenantDto dto);
        Task<ResultData<bool>> DeleteTenantAsync(int tenantId);
    }

}

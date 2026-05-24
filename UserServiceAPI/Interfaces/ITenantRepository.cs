using UserServiceAPI.DTOs.Tenant;

namespace UserServiceAPI.Interfaces
{
    public interface ITenantRepository
    {
        Task<ResultData<IEnumerable<TenantDto>>> GetAllTenantsAsync();
        Task<ResultData<TenantDto>> GetTenantByIdAsync(int tenantId);
        Task<ResultData<int>> CreateTenantAsync(TenantDto dto);
        Task<ResultData<bool>> UpdateTenantAsync(TenantDto dto);
        Task<ResultData<bool>> DeleteTenantAsync(int tenantId);
    }

}

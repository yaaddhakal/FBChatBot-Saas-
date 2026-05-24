using CoreCommon.DbService;
using System.Data;
using UserServiceAPI.DTOs;
using UserServiceAPI.DTOs.Tenant;
using UserServiceAPI.Interfaces;

public class TenantRepository : ITenantRepository
{
    private readonly IDbService _dbService;

    public TenantRepository(IDbService dbService)
    {
        _dbService = dbService;
    }

    public async Task<ResultData<IEnumerable<TenantDto>>> GetAllTenantsAsync()
    {
        var result = await _dbService.GetAsync<IEnumerable<TenantDto>>(
            "sp_Tenants_CRUD",
            new { Flag = "SELECTALL" },
            CommandType.StoredProcedure);

        return result.Data != null
            ? ResultData<IEnumerable<TenantDto>>.Ok(result.Data)
            : ResultData<IEnumerable<TenantDto>>.Fail("No tenants found");
    }

    public async Task<ResultData<TenantDto>> GetTenantByIdAsync(int tenantId)
    {
        var result = await _dbService.GetAsync<TenantDto>(
            "sp_Tenants_CRUD",
            new { Flag = "SELECTBYID", TenantID = tenantId },
            CommandType.StoredProcedure);

        return result.Data != null
            ? ResultData<TenantDto>.Ok(result.Data)
            : ResultData<TenantDto>.Fail("Tenant not found");
    }

    public async Task<ResultData<int>> CreateTenantAsync(TenantDto dto)
    {
        var result = await _dbService.GetAsync<SpResult>(
            "sp_Tenants_CRUD",
            new
            {
                Flag = "CREATE",
                dto.TenantName,
                dto.ContactEmail,
                dto.Phno,
                dto.IsActive,
                dto.IsRecorded,
                dto.Col1,
                dto.Col2
               
            },
            CommandType.StoredProcedure);

        return result.Data?.Result > 0
            ? ResultData<int>.Ok(result.Data.Result)
            : ResultData<int>.Fail(result.Data?.Message ?? "Failed to create tenant");
    }

    public async Task<ResultData<bool>> UpdateTenantAsync(TenantDto dto)
    {
        var result = await _dbService.GetAsync<SpResult>(
            "sp_Tenants_CRUD",
            new
            {
                Flag = "UPDATE",
                dto.TenantID,
                dto.TenantName,
                dto.ContactEmail,
                dto.Phno,
                dto.IsActive,
                dto.IsRecorded,
                dto.Col1,
                dto.Col2
            },
            CommandType.StoredProcedure);

        return result.Data?.Result > 0
            ? ResultData<bool>.Ok(true)
            : ResultData<bool>.Fail(result.Data?.Message ?? "Failed to update tenant");
    }

    public async Task<ResultData<bool>> DeleteTenantAsync(int tenantId)
    {
        var result = await _dbService.GetAsync<SpResult>(
            "sp_Tenants_CRUD",
            new { Flag = "DELETE", TenantID = tenantId },
            CommandType.StoredProcedure);

        return result.Data?.Result > 0
            ? ResultData<bool>.Ok(true)
            : ResultData<bool>.Fail(result.Data?.Message ?? "Failed to delete tenant");
    }
}

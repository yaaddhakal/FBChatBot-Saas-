using Microsoft.AspNetCore.Mvc;
using UserServiceAPI.DTOs.Tenant;
using UserServiceAPI.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ILogger<TenantController> _logger;
    private readonly ITenantRepository _tenantRepository;

    public TenantController(ILogger<TenantController> logger, ITenantRepository tenantRepository)
    {
        _logger = logger;
        _tenantRepository = tenantRepository;
    }

    [HttpGet("GetAllTenantsAsync")]
    public async Task<IActionResult> GetAllTenantsAsync()
    {
        _logger.LogInformation("GetAllTenantsAsync called");
        var result = await _tenantRepository.GetAllTenantsAsync();
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("GetTenantByIdAsync/{id}")]
    public async Task<IActionResult> GetTenantByIdAsync(int id)
    {
        _logger.LogInformation("GetTenantByIdAsync called with id={Id}", id);

        var result = await _tenantRepository.GetTenantByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("CreateTenantAsync")]
    public async Task<IActionResult> CreateTenantAsync([FromBody] TenantDto tenant)
    {
        _logger.LogInformation("CreateTenantAsync called");

        var result = await _tenantRepository.CreateTenantAsync(tenant);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("UpdateTenantAsync/{id}")]
    public async Task<IActionResult> UpdateTenantAsync(int id, [FromBody] TenantDto tenant)
    {
        _logger.LogInformation("UpdateTenantAsync called with id={Id}", id);

        tenant.TenantID = id;
        var result = await _tenantRepository.UpdateTenantAsync(tenant);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("DeleteTenantAsync/{id}")]
    public async Task<IActionResult> DeleteTenantAsync(int id)
    {
        _logger.LogInformation("DeleteTenantAsync called with id={Id}", id);

        var result = await _tenantRepository.DeleteTenantAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

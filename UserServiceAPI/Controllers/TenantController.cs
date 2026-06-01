using CoreCommon.HelperCommon;
using Microsoft.AspNetCore.Mvc;
using UserServiceAPI.DTOs.Tenant;
using UserServiceAPI.Interfaces;

namespace UserServiceAPI.Controllers
{
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
        [HttpPost("signup")]
        public async Task<IActionResult> SignupTenantAsync([FromBody] SignupTenantRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tenantRepository.SignupTenantAsync(request);

            if (!result.Success)
                _logger.LogWarning("[Signup] Failed for user: {Username}", request.UserName);

            return ActionResultHelper.FromResult(this, result);
        }
        [HttpGet("GetAllTenantsListAsync")]
        public async Task<IActionResult> GetAllTenantsListAsync()
        {
            var result = await _tenantRepository.GetAllTenantsListAsync();
            return ActionResultHelper.FromResult(this, result);
        }

        [HttpGet("industries/{tenantID}")]
        public async Task<IActionResult> GetIndustriesByTenantAsync(int tenantID)
        {
            var result = await _tenantRepository.GetIndustriesByTenantAsync(tenantID);
            return ActionResultHelper.FromResult(this, result);
        }
        [HttpGet("GetAllTenantsAsync")]
        public async Task<IActionResult> GetAllTenantsAsync()
        {
            _logger.LogInformation("GetAllTenantsAsync called");
            var result = await _tenantRepository.GetAllTenantsAsync();
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("GetTenantByIdAsync")]
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

        [HttpPut("UpdateTenantAsync")]
        public async Task<IActionResult> UpdateTenantAsync([FromBody] TenantDto tenant)
        {
            _logger.LogInformation("UpdateTenantAsync called with id={Id}", tenant.TenantID);

           
            var result = await _tenantRepository.UpdateTenantAsync(tenant);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpDelete("DeleteTenantAsync")]
        public async Task<IActionResult> DeleteTenantAsync(int id)
        {
            _logger.LogInformation("DeleteTenantAsync called with id={Id}", id);

            var result = await _tenantRepository.DeleteTenantAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserServiceAPI.Interfaces;

namespace UserServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("GetAllTenantsAsync")]
        public async Task<IActionResult> GetAllTenantsAsync()
        {
            return null;
        }
    }
}

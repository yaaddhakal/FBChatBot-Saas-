using AuthAPI.Interfaces;

using System.Security.Claims;

namespace AuthAPI.Repositories
{
    public class ClaimsRepository : IClaimsRepository
    {
        public string? GetUserId(ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? GetUsername(ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Name)?.Value;

        public string? GetUserRole(ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Role)?.Value
            ?? user.FindFirst("role")?.Value;

        public string? GetDepartment(ClaimsPrincipal user) =>
            user.FindFirst("department")?.Value;

        public string? GetCompanyId(ClaimsPrincipal user) =>
            user.FindFirst("companyId")?.Value;

        public dynamic GetCurrentUserByClaim(ClaimsPrincipal user) => new
        {
            UserId = GetUserId(user),
            Username = GetUsername(user),
            Role = GetUserRole(user),
            Department = GetDepartment(user),
            CompanyId = GetCompanyId(user)
        };

       
    }

}

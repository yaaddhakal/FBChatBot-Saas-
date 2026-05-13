using System.Security.Claims;

namespace AuthAPI.Interfaces
{
    public interface IClaimsRepository
    {
        /// <summary>
        /// Get claims for a user by their ID
        /// </summary>
        string? GetUserId(ClaimsPrincipal user);
        string? GetUsername(ClaimsPrincipal user);
        string? GetUserRole(ClaimsPrincipal user);
        string? GetDepartment(ClaimsPrincipal user);
        string? GetCompanyId(ClaimsPrincipal user);
        dynamic GetCurrentUserByClaim(ClaimsPrincipal user);

    }
}

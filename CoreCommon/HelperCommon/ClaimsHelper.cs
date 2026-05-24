using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CoreCommon.HelperCommon
{
    public class ClaimsHelper
    {
        public static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? GetUsername(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }
        public static string? GetUserRoleId(ClaimsPrincipal user)
        {
            return user.FindFirst("roleId")?.Value;
        }
        public static string? GetUserRole(ClaimsPrincipal user)
        {
            // Works with ClaimTypes.Role or "role"
            return user.FindFirst(ClaimTypes.Role)?.Value
                   ?? user.FindFirst("role")?.Value;
        }
        public static string? GetTenantId(ClaimsPrincipal user)
        {
            return user.FindFirst("tenantId")?.Value;
        }

        public static string? GetTenantName(ClaimsPrincipal user)
        {
            return user.FindFirst("tenantName")?.Value;
        }

        public static string? GetIndustryId(ClaimsPrincipal user)
        {
            return user.FindFirst("industryId")?.Value;
        }

        public static string? GetIndustryName(ClaimsPrincipal user)
        {
            return user.FindFirst("industryName")?.Value;
        }

        public static string? GetUserType(ClaimsPrincipal user)
        {
            return user.FindFirst("userType")?.Value;
        }
        public CurrentUserInfo GetCurrentUserByClaim(ClaimsPrincipal user)
        {
            var userInfo = new CurrentUserInfo
            {
                UserID = ClaimsHelper.GetUserId(user),
                UserName = ClaimsHelper.GetUsername(user),
                RoleID = ClaimsHelper.GetUserRoleId(user),
                Role = ClaimsHelper.GetUserRole(user),
                TenantID = ClaimsHelper.GetTenantId(user),
                TenantName = ClaimsHelper.GetTenantName(user),
                IndustryID = ClaimsHelper.GetIndustryId(user),
                IndustryName = ClaimsHelper.GetIndustryName(user),
                UserType = ClaimsHelper.GetUserType(user)
            };

            return userInfo;
        }
        public class CurrentUserInfo
        {
            public string? UserID { get; set; }
            public string? UserName { get; set; }
            public string? RoleID { get; set; }
            public string? Role { get; set; }
            public string? TenantID { get; set; }
            public string? TenantName { get; set; }
            public string? IndustryID { get; set; }
            public string? IndustryName { get; set; }
            public string? UserType { get; set; }
        }
    }
}

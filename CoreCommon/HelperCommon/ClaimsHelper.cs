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
        public static string? GetDepartmentId(ClaimsPrincipal user)
        {
            return user.FindFirst("departmentId")?.Value;
        }
        public static string? GetDepartment(ClaimsPrincipal user)
        {
            return user.FindFirst("department")?.Value;
        }

        public static string? GetCompanyId(ClaimsPrincipal user)
        {
            return user.FindFirst("companyId")?.Value;
        }
        public static string? GetCompanyName(ClaimsPrincipal user)
        {
            return user.FindFirst("companyName")?.Value;
        }
        public CurrentUserInfo GetCurrentUserByClaim(ClaimsPrincipal user)
        {
            var userInfo = new CurrentUserInfo
            {
                UserId = ClaimsHelper.GetUserId(user),
                Username = ClaimsHelper.GetUsername(user),
                RoleId = ClaimsHelper.GetUserRoleId(user),
                Role = ClaimsHelper.GetUserRole(user),
                DepartmentId = ClaimsHelper.GetDepartmentId(user),
                Department = ClaimsHelper.GetDepartment(user),
                CompanyId = ClaimsHelper.GetCompanyId(user),
                CompanyName = ClaimsHelper.GetCompanyName(user)
            };

            return userInfo;
        }
        public class CurrentUserInfo
        {
            public string? UserId { get; set; }
            public string? Username { get; set; }
            public string? RoleId { get; set; }
            public string? Role { get; set; }
            public string? DepartmentId { get; set; }
            public string? Department { get; set; }
            public string? CompanyId { get; set; }
            public string? CompanyName { get; set; }
        }
    }
}

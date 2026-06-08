using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCommon.Models.UsersModels
{
    public class UserViewDto
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        // Tenant & Industry
        public int TenantIndustryID { get; set; }
        public int IndustryID { get; set; }
        public string? IndustryName { get; set; }
        public int TenantID { get; set; }
        public string? TenantName { get; set; }

        // User status
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        // Role
        public int RoleID { get; set; }
        public string? RoleName { get; set; }
        public string? UserType { get; set; }
    }

}

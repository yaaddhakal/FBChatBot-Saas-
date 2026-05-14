using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCommon.Models.ViewModels
{
    public class UsersViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int IndustryID { get; set; }
        public string IndustryName { get; set; } = string.Empty;
        public int TenantID  { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }

        // Role fields
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
    }
}

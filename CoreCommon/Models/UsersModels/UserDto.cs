using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCommon.Models.UsersModels
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string? UserType { get; set; }
        public int TenantIndustryID { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        // Extra columns for flexibility
        public string? Col1 { get; set; }
        public string? Col2 { get; set; }
    }

}

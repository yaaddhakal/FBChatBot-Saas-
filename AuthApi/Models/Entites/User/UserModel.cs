using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthAPI.Models.Entites.Company;
using AuthAPI.Models.Entites.Department;
using AuthAPI.Models.Entites.Role;
namespace AuthAPI.Models.Entites.User
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }=string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int CompanyId { get; set; }
        public DateTime DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public DateTime IssuedDate_From { get; set; }
        public DateTime? IssuedDate_To { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RoleName { get; set; } = "User";
        public string DepartmentName { get; set; } = "General";

        public RoleModel? Role { get; set; }
        public CompanyModel? Company { get; set; }

    }
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int CompanyId { get; set; }
        public DateTime DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public DateTime IssuedDate_From { get; set; }
        public DateTime? IssuedDate_To { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DepartmentId { get; set; }
        public string RoleName { get; set; } = "User";
        public string DepartmentName { get; set; } = "General";
        public string CompanyName { get; set; } = "General";



    }

}

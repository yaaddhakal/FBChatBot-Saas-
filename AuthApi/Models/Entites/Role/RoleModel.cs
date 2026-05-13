using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthAPI.Models.Entites.Department;
namespace AuthAPI.Models.Entites.Role
{
    public class RoleModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }=string.Empty;
        public int DepartmentId { get; set; }
        public string Role_Summary { get; set; }=string.Empty;
        public DateTime CreatedAt { get; set; }

        public DepartmentModel? Department { get; set; }

    }
}

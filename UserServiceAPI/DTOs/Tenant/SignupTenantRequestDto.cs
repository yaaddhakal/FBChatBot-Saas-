using System.ComponentModel.DataAnnotations;

namespace UserServiceAPI.DTOs.Tenant
{
    public class SignupTenantRequestDto
    {
        public int? TenantID { get; set; }
        public int? IndustryID { get; set; }
        public string TenantName { get; set; }=string.Empty;
        public string IndustryName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }
    
}

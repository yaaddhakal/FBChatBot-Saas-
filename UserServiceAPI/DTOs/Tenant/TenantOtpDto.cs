using System.ComponentModel.DataAnnotations;

namespace UserServiceAPI.DTOs.Tenant
{
    public class TenantOtpDto
    {
        public int OtpID { get; set; }
        public int UserId { get; set; }
        public string OtpCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
    public class ResendOtpRequestDto
    {
        [Required]
        public int UserID { get; set; }
    }
}

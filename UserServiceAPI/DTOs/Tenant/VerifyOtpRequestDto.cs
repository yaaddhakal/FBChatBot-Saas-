using System.ComponentModel.DataAnnotations;

namespace UserServiceAPI.DTOs.Tenant
{
    public class VerifyOtpRequestDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; }
    }
}

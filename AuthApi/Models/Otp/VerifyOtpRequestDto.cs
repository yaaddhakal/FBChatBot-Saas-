using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models.Otp
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

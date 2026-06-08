namespace AuthAPI.Models.Otp
{
    public class OtpDto
    {
        public int OtpID { get; set; }
        public int UserId { get; set; }
        public string OtpCode { get; set; }=string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}

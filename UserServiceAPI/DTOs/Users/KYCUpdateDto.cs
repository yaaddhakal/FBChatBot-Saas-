namespace UserServiceAPI.DTOs.Users
{
    public class KYCUpdateDto
    {
        public int UserId { get; set; }
        public required string KYCStatus { get; set; }  // Pending/Verified/Rejected
        public string? KYCDocument { get; set; }
    }
}

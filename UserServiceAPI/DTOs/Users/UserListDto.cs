namespace UserServiceAPI.DTOs.Users
{
        public class UserListDto
        {
            public int UserId { get; set; }
            public string? FullName { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? MobileNo { get; set; }
            public int RoleId { get; set; }
            public int CompanyId { get; set; }
            public bool IsActive { get; set; }
            public string? KYCStatus { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }

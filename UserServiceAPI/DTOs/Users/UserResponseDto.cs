namespace UserServiceAPI.DTOs.Users
{
        public class UserResponseDto
        {
            public int UserId { get; set; }
            public string? FullName { get; set; }
            public string? Username { get; set; }
            public int RoleId { get; set; }
            public int CompanyId { get; set; }
            public DateTime? DOB { get; set; }
            public string? Gender { get; set; }
            public string? Email { get; set; }
            public string? MobileNo { get; set; }
            public DateTime? IssuedDate_From { get; set; }
            public DateTime? IssuedDate_To { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }

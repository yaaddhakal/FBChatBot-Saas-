namespace UserServiceAPI.DTOs.Users
{
        public class ChangePasswordDto
        {
            public int UserId { get; set; }
            public required string CurrentPassword { get; set; }
            public required string NewPassword { get; set; }
            public required string ConfirmPassword { get; set; }
        }
    
}

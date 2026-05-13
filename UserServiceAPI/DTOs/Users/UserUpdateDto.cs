namespace UserServiceAPI.DTOs.Users
{
 
        public class UserUpdateDto
        {
            public int UserId { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? MobileNo { get; set; }
            public int RoleId { get; set; }
            public int CompanyId { get; set; }
            public DateTime? DOB { get; set; }
            public string? Gender { get; set; }
            public DateTime? IssuedDate_From { get; set; }
            public DateTime? IssuedDate_To { get; set; }
            public bool IsActive { get; set; }

            // UserDetail fields
            public string? ImageLocation { get; set; }
            public string? Country { get; set; }
            public string? State { get; set; }
            public string? City { get; set; }
            public string? Address { get; set; }
            public string? PostalCode { get; set; }
            public string? NationalId { get; set; }
            public string? Occupation { get; set; }
            public string? BloodGroup { get; set; }
            public string? AlternateMobile { get; set; }
            public string? EmergencyContact { get; set; }
            public decimal AccountLimit { get; set; }
            public string? KYCStatus { get; set; }
            public string? KYCDocument { get; set; }
        }
    }

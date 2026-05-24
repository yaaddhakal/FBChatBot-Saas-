namespace UserServiceAPI.DTOs.Tenant
{
    public class TenantDto
    {
        public int TenantID { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? Phno { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsRecorded { get; set; }
        public string? Col1 { get; set; }
        public string? Col2 { get; set; }
    }

}

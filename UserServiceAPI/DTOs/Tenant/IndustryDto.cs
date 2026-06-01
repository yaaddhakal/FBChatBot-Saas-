namespace UserServiceAPI.DTOs.Tenant
{
    public class IndustryDto
    {
        public int IndustryID { get; set; }
        public string IndustryName { get; set; }=string.Empty;
        public string? ContactEmail {  get; set; }=string.Empty; 
        public string Phnos { get; set; }=string.Empty;
        public string? Col1 { get; set; } = string.Empty;
        public string? Col2 { get; set; } = string.Empty;
    }
}

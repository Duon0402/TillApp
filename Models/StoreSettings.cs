namespace TillApp.Models
{
    public class StoreSettings
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
    }
}

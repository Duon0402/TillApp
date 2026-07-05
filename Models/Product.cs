namespace TillApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int MinStock { get; set; }
        public int CurrentStock { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}

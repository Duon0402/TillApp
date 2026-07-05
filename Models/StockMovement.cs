namespace TillApp.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Qty { get; set; }
        public StockMovementType Type { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

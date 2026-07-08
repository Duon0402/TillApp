namespace TillApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineDiscount { get; set; }
    }
}

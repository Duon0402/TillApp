namespace TillApp.Models
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrderValue { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int MaxUsage { get; set; }
        public int UsedCount { get; set; }
    }

    public enum VoucherType
    {
        PercentOff,
        FixedAmount
    }
}

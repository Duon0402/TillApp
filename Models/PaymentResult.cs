namespace TillApp.Models
{
    public record PaymentResult(bool Confirmed, string PaymentMethod, decimal CashReceived);
}

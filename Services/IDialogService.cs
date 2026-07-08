using TillApp.Models;

namespace TillApp.Services
{
    public interface IDialogService
    {
        PaymentResult? ShowPaymentDialog(decimal total);
    }
}

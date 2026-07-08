using TillApp.Models;
using TillApp.ViewModels;
using TillApp.Views;

namespace TillApp.Services
{
    public class DialogService : IDialogService
    {
        public PaymentResult? ShowPaymentDialog(decimal total)
        {
            var vm = new PaymentDialogViewModel(total);
            var window = new PaymentDialogView { DataContext = vm };

            var ok = window.ShowDialog();
            if (ok != true || !vm.Confirmed)
                return null;

            return new PaymentResult(true, vm.SelectedMethod, vm.CashReceived);
        }
    }
}

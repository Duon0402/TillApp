using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TillApp.ViewModels
{
    public partial class PaymentDialogViewModel : BaseViewModel
    {
        public decimal Total { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ChangeAmount))]
        private string _selectedMethod = "Cash";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ChangeAmount))]
        private decimal _cashReceived;

        public decimal ChangeAmount => SelectedMethod == "Cash" ? Math.Max(0, CashReceived - Total) : 0;

        public bool Confirmed { get; private set; }

        public PaymentDialogViewModel(decimal total)
        {
            Total = total;
        }

        [RelayCommand]
        private void Confirm() => Confirmed = true;
    }
}

using System.Windows;
using TillApp.ViewModels;

namespace TillApp.Views;

public partial class PaymentDialogView : Window
{
    public PaymentDialogView()
    {
        InitializeComponent();
    }

    private void CashRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is PaymentDialogViewModel vm)
            vm.SelectedMethod = "Cash";

        if (CashPanel != null)
            CashPanel.Visibility = Visibility.Visible;
    }

    private void TransferRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is PaymentDialogViewModel vm)
            vm.SelectedMethod = "BankTransfer";

        if (CashPanel != null)
            CashPanel.Visibility = Visibility.Collapsed;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is PaymentDialogViewModel vm)
        {
            if (vm.SelectedMethod == "Cash" && vm.CashReceived < vm.Total)
            {
                MessageBox.Show("Số tiền khách đưa không đủ.", "Thanh toán");
                return;
            }

            vm.ConfirmCommand.Execute(null);
        }

        DialogResult = true;
        Close();
    }
}

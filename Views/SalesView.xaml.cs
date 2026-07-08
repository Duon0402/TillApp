using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TillApp.ViewModels;

namespace TillApp.Views
{
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalesViewModel vm)
                await vm.LoadAsync();

            BarcodeTextBox.Focus();
        }

        private async void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is SalesViewModel vm)
            {
                await vm.ScanBarcodeCommand.ExecuteAsync(null);
                BarcodeTextBox.Focus();
            }
        }
    }
}

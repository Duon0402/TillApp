using System.Windows.Controls;
using TillApp.ViewModels;

namespace TillApp.Views
{
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SalesViewModel vm)
                await vm.LoadAsync();
        }
    }
}

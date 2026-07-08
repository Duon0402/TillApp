using System.Windows;
using System.Windows.Controls;
using TillApp.ViewModels;

namespace TillApp.Views;

public partial class ProductListView : UserControl
{
    public ProductListView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductListViewModel vm)
            await vm.LoadAsync();
    }
}

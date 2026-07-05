using System.Windows.Controls;
using TillApp.ViewModels;

namespace TillApp.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            await vm.LoadAsync();
    }
}

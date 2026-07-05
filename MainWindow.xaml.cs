using System.Windows;
using TillApp.ViewModels;

namespace TillApp;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ = viewModel.InitializeAsync();
    }
}

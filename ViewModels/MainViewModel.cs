using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TillApp.Features;
using TillApp.Services;

namespace TillApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        public INavigationService Navigation { get; }

        [ObservableProperty]
        private string _pingResult = "Not called yet";

        private readonly IMediator _mediator;

        public MainViewModel(INavigationService navigation, IMediator mediator)
        {
            Navigation = navigation;
            _mediator = mediator;
        }

        public async Task InitializeAsync()
        {
            PingResult = await _mediator.Send(new PingCommand());
            Navigation.NavigateTo<LoginViewModel>();
        }

        [RelayCommand]
        private void NavigateToProducts() => Navigation.NavigateTo<ProductListViewModel>();

        [RelayCommand]
        private void NavigateToSales() => Navigation.NavigateTo<SalesViewModel>();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TillApp.Features;
using TillApp.Services;

namespace TillApp.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigation;
        private readonly ISessionService _sessionService;

        [ObservableProperty]
        private string _username = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string? _errorMessage = null;

        public LoginViewModel(IMediator mediator, INavigationService navigation, ISessionService sessionService)
        {
            _mediator = mediator;
            _navigation = navigation;
            _sessionService = sessionService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            var result = await _mediator.Send(new LoginCommand(Username, Password));

            if (result.Success == false)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _sessionService.SetUser(Username, result.Role);
            ErrorMessage = $"Đăng nhập thành công — role: {result.Role}";
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TillApp.Features;

namespace TillApp.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly IMediator _mediator;

        [ObservableProperty] private string _name = "";
        [ObservableProperty] private string _address = "";
        [ObservableProperty] private string _phone = "";
        [ObservableProperty] private string _taxCode = "";
        [ObservableProperty] private string? _logoPath;
        [ObservableProperty] private string? _statusMessage;

        public SettingsViewModel(IMediator mediator) => _mediator = mediator;

        public async Task LoadAsync()
        {
            var settings = await _mediator.Send(new GetStoreSettingsQuery());
            if (settings is null) return;

            Name = settings.Name;
            Address = settings.Address;
            Phone = settings.Phone;
            TaxCode = settings.TaxCode;
            LogoPath = settings.LogoPath;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            await _mediator.Send(new SaveStoreSettingsCommand(Name, Address, Phone, TaxCode, LogoPath));
            StatusMessage = "Đã lưu cấu hình cửa hàng";
        }
    }
}

using TillApp.ViewModels;

namespace TillApp.Services
{
    public interface INavigationService
    {
        BaseViewModel CurrentViewModel { get; }
        void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
    }
}

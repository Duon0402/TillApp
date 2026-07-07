using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using TillApp.Features;
using TillApp.Models;

namespace TillApp.ViewModels
{
    public partial class ProductListViewModel : BaseViewModel
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private ObservableCollection<ProductDto> _products = new();

        [ObservableProperty]
        private string _searchText = "";

        public ProductListViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task LoadAsync()
        {
            await SearchAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchAsync();
        }

        private async Task SearchAsync()
        {
            var result = await _mediator.Send(new GetProductsQuery(SearchText, null));
            Products = new ObservableCollection<ProductDto>(result.Items);
        }
    }
}

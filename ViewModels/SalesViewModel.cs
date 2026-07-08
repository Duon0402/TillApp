using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using System.Collections.ObjectModel;
using TillApp.Features;
using TillApp.Models;

namespace TillApp.ViewModels
{
    public partial class SalesViewModel : BaseViewModel
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private ObservableCollection<ProductDto> _products = [];

        // Giỏ hàng thật sự (CartItemViewModel) sẽ làm ở bước 3.2
        public decimal GrandTotal => 0;

        public SalesViewModel(IMediator mediator) => _mediator = mediator;

        public async Task LoadAsync()
        {
            var result = await _mediator.Send(new GetProductsQuery(null, null, PageSize: 100));

            Products = new ObservableCollection<ProductDto>(result.Items);
        }
    }
}

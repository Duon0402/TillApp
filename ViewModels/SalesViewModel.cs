using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using TillApp.Features;
using TillApp.Models;

namespace TillApp.ViewModels
{
    public partial class SalesViewModel : BaseViewModel
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private ObservableCollection<ProductDto> _products = [];

        [ObservableProperty]
        private ObservableCollection<CartItemViewModel> _cartItems = [];

        [ObservableProperty]
        private string _barcodeInput = string.Empty;

        public decimal GrandTotal => CartItems.Sum(i => i.LineTotal);

        public SalesViewModel(IMediator mediator) => _mediator = mediator;

        public async Task LoadAsync()
        {
            var result = await _mediator.Send(new GetProductsQuery(null, null, PageSize: 100));

            Products = new ObservableCollection<ProductDto>(result.Items);
        }

        [RelayCommand]
        private void AddToCart(ProductDto product)
        {
            AddOrIncrease(product);
            OnPropertyChanged(nameof(GrandTotal));
        }

        [RelayCommand]
        private void RemoveFromCart(CartItemViewModel item)
        {
            item.PropertyChanged -= CartItem_PropertyChanged;
            CartItems.Remove(item);
            OnPropertyChanged(nameof(GrandTotal));
        }

        private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItemViewModel.LineTotal))
                OnPropertyChanged(nameof(GrandTotal));
        }

        [RelayCommand]
        private async Task ScanBarcode()
        {
            if (string.IsNullOrWhiteSpace(BarcodeInput))
                return;

            var product = await _mediator.Send(new GetProductByBarcodeQuery(BarcodeInput));

            if (product is not null)
                AddOrIncrease(product);
            else
                MessageBox.Show($"Không tìm thấy sản phẩm với mã '{BarcodeInput}'", "Quét mã vạch");

            BarcodeInput = string.Empty;
        }

        private void AddOrIncrease(ProductDto product)
        {
            var existing = CartItems.FirstOrDefault(i => i.ProductId == product.Id);

            if (existing != null)
            {
                existing.Qty++;
            }
            else
            {
                var item = new CartItemViewModel(product.Id, product.Name, product.Price);
                item.PropertyChanged += CartItem_PropertyChanged;
                CartItems.Add(item);
            }
        }
    }
}

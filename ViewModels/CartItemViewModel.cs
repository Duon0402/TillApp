using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TillApp.ViewModels
{
    public partial class CartItemViewModel : BaseViewModel
    {
        public int ProductId { get; }
        public string Name { get; }
        public decimal UnitPrice { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LineTotal))]
        private int _qty = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LineTotal))]
        private decimal _lineDiscount = 0;

        public decimal LineTotal => (UnitPrice * Qty) - LineDiscount;

        public CartItemViewModel(int productId, string name, decimal unitPrice)
        {
            ProductId = productId;
            Name = name;
            UnitPrice = unitPrice;
        }

        [RelayCommand]
        private void Increase() => Qty++;

        [RelayCommand]
        private void Decrease()
        {
            if (Qty > 1)
                Qty--;
        }
    }
}
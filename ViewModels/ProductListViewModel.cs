using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
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
        private string _searchText = string.Empty;

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

        [RelayCommand]
        private async Task ImportCsv()
        {
            var dialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };
            if (dialog.ShowDialog() != true) return;

            var result = await _mediator.Send(new BulkImportProductsCommand(dialog.FileName));

            var message = new StringBuilder();
            message.AppendLine($"Thành công: {result.SuccessCount} dòng");
            message.AppendLine($"Lỗi: {result.FailCount} dòng");
            foreach (var err in result.Errors.Take(10))
                message.AppendLine($"- Dòng {err.Line}: {err.Message}");

            MessageBox.Show(message.ToString(), "Kết quả nhập CSV");
            await SearchAsync();
        }
    }
}

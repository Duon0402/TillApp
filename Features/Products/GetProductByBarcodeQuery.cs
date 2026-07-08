using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record GetProductByBarcodeQuery(string Barcode) : IRequest<ProductDto?>;

    public class GetProductByBarcodeHandler : IRequestHandler<GetProductByBarcodeQuery, ProductDto?>
    {
        private readonly AppDbContext _db;

        public GetProductByBarcodeHandler(AppDbContext db) => _db = db;

        public async Task<ProductDto?> Handle(GetProductByBarcodeQuery q, CancellationToken ct)
        {
            var product = await _db.Products
                .Where(p => p.Barcode == q.Barcode)
                .Select(p => new ProductDto(p.Id, p.Name, p.Barcode, p.Price, p.CurrentStock, p.Category.Name, p.CurrentStock <= p.MinStock))
                .FirstOrDefaultAsync(ct);
            return product;
        }
    }
}

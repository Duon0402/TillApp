using MediatR;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record CreateProductCommand(
        string Name, string Barcode, decimal Price, decimal CostPrice,
        string Unit, int MinStock, int CategoryId
    ) : IRequest<int>;

    public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly AppDbContext _db;

        public CreateProductHandler(AppDbContext db) => _db = db;

        public async Task<int> Handle(CreateProductCommand cmd, CancellationToken ct)
        {
            var product = new Product
            {
                Name = cmd.Name,
                Barcode = cmd.Barcode,
                Price = cmd.Price,
                CostPrice = cmd.CostPrice,
                Unit = cmd.Unit,
                MinStock = cmd.MinStock,
                CategoryId = cmd.CategoryId,
                CurrentStock = 0
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync(ct);
            return product.Id;
        }
    }
}

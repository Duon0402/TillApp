using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;

namespace TillApp.Features
{
    public record UpdateProductCommand(
        int Id, string Name, string Barcode, decimal Price, decimal CostPrice,
        string Unit, int MinStock, int CategoryId
    ) : IRequest<Unit>;

    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Unit>
    {
        private readonly AppDbContext _db;

        public UpdateProductHandler(AppDbContext db) => _db = db;

        public async Task<Unit> Handle(UpdateProductCommand cmd, CancellationToken ct)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == cmd.Id, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm có Id {cmd.Id}.");

            product.Name = cmd.Name;
            product.Barcode = cmd.Barcode;
            product.Price = cmd.Price;
            product.CostPrice = cmd.CostPrice;
            product.Unit = cmd.Unit;
            product.MinStock = cmd.MinStock;
            product.CategoryId = cmd.CategoryId;
            await _db.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
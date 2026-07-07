using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record AdjustStockCommand(int ProductId, int Qty, StockMovementType Type, string? Note) : IRequest<Unit>;

    public class AdjustStockHandler : IRequestHandler<AdjustStockCommand, Unit>
    {
        private readonly AppDbContext _db;
        public AdjustStockHandler(AppDbContext db) => _db = db;

        public async Task<Unit> Handle(AdjustStockCommand cmd, CancellationToken ct)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == cmd.ProductId, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm có Id {cmd.ProductId}.");

            _db.StockMovements.Add(new StockMovement
            {
                ProductId = cmd.ProductId,
                Qty = cmd.Qty,
                Type = cmd.Type,
                Note = cmd.Note,
                CreatedAt = DateTime.Now
            });

            product.CurrentStock += cmd.Type == StockMovementType.In ? cmd.Qty : -cmd.Qty;

            await _db.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    public class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
    {
        public AdjustStockValidator(AppDbContext db)
        {
            RuleFor(x => x.Qty).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");

            RuleFor(x => x).MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != StockMovementType.Out) return true;
                var product = await db.Products.FindAsync([cmd.ProductId], ct);
                return product != null && product.CurrentStock >= cmd.Qty;
            }).WithMessage("Không đủ tồn kho để xuất");
        }
    }
}

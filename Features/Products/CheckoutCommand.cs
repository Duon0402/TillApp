using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record CheckoutItemDto(int ProductId, int Qty, decimal UnitPrice, decimal LineDiscount);
    public record CheckoutCommand(List<CheckoutItemDto> Items, string PaymentMethod) : IRequest<int>;

    public class CheckoutHandler : IRequestHandler<CheckoutCommand, int>
    {
        private readonly AppDbContext _db;

        public CheckoutHandler(AppDbContext db) => _db = db;

        public async Task<int> Handle(CheckoutCommand cmd, CancellationToken ct)
        {
            var order = new Order
            {
                CreatedAt = DateTime.Now,
                PaymentMethod = cmd.PaymentMethod,
                Total = cmd.Items.Sum(i => (i.UnitPrice * i.Qty) - i.LineDiscount),
            };

            _db.Orders.Add(order);

            foreach (var item in cmd.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Qty = item.Qty,
                    UnitPrice = item.UnitPrice,
                    LineDiscount = item.LineDiscount
                });

                var product = await _db.Products.FirstAsync(p => p.Id == item.ProductId, ct);
                product.CurrentStock -= item.Qty;

                _db.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    Qty = item.Qty,
                    Type = StockMovementType.Out,
                    Note = "Bán hàng",
                    CreatedAt = DateTime.Now
                });
            }

            await _db.SaveChangesAsync(ct);
            return order.Id;
        }
    }

    public class CheckoutValidator : AbstractValidator<CheckoutCommand>
    {
        public CheckoutValidator(AppDbContext db)
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("Danh sách sản phẩm không được để trống");

            RuleFor(x => x).MustAsync(async (cmd, ct) =>
            {
                var ids = cmd.Items.Select(i => i.ProductId).ToList();
                var stocks = await db.Products.Where(p => ids.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.CurrentStock, ct);

                return cmd.Items.All(i => stocks.TryGetValue(i.ProductId, out var stock) && i.Qty <= stock);
            }).WithMessage("Có sản phẩm không đủ tồn kho để bán");

            RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("Phương thức thanh toán không được để trống");
        }
    }
}

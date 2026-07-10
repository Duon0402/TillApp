using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record VoucherApplyCommand(string Code, decimal OrderTotal) : IRequest<VoucherApplyResult>;
    public record VoucherApplyResult(bool Success, decimal DiscountAmount, string? ErrorMessage);

    public class VoucherApplyHandler : IRequestHandler<VoucherApplyCommand, VoucherApplyResult>
    {
        private readonly AppDbContext _db;

        public VoucherApplyHandler(AppDbContext db) => _db = db;

        public async Task<VoucherApplyResult> Handle(VoucherApplyCommand cmd, CancellationToken ct)
        {
            var voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Code == cmd.Code, ct);

            if (voucher == null)
            {
                return new VoucherApplyResult(false, 0, $"Không tìm thấy voucher: {cmd.Code}");
            }

            if (voucher.ExpiresAt < DateTime.Now)
            {
                return new VoucherApplyResult(false, 0, $"Voucher {cmd.Code} đã hết hạn sử dụng.");
            }

            if (voucher.UsedCount >= voucher.MaxUsage)
            {
                return new VoucherApplyResult(false, 0, $"Voucher {cmd.Code} đã đạt số lần sử dụng tối đa.");
            }

            if (cmd.OrderTotal < voucher.MinOrderValue)
            {
                return new VoucherApplyResult(false, 0,
                    $"Đơn hàng cần tối thiểu {voucher.MinOrderValue:N0} đ để áp dụng voucher {cmd.Code}.");
            }

            var discount = voucher.Type switch
            {
                VoucherType.PercentOff => cmd.OrderTotal * (voucher.Value / 100),
                VoucherType.FixedAmount => voucher.Value,
                _ => 0
            };

            return new VoucherApplyResult(true, discount, null);
        }
    }
}

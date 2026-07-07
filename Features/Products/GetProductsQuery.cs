using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record GetProductsQuery(
        string? Search,
        int? CategoryId,
        bool LowStockOnly = false,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<ProductDto>>;

    public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
    {
        private readonly AppDbContext _db;

        public GetProductsHandler(AppDbContext db) => _db = db;

        public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery q, CancellationToken ct)
        {
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q.Search))
                query = query.Where(x => x.Name.Contains(q.Search) || x.Barcode.Contains(q.Search));

            if (q.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == q.CategoryId);

            if (q.LowStockOnly)
                query = query.Where(p => p.CurrentStock <= p.MinStock);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(p => new ProductDto(p.Id, p.Name, p.Barcode, p.Price, p.CurrentStock, p.Category.Name, p.CurrentStock <= p.MinStock))
                .ToListAsync(ct);

            return new PagedResult<ProductDto>(items, totalCount, q.PageNumber, q.PageSize);
        }
    }
}

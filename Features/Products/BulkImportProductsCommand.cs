using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record BulkImportProductsCommand(string FilePath) : IRequest<BulkImportResult>;

    public record ImportError(int Line, string Message);

    public record BulkImportResult(int SuccessCount, int FailCount, List<ImportError> Errors);

    public class CsvProductRow
    {
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int MinStock { get; set; }
        public int CurrentStock { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class BulkImportProductsHandler : IRequestHandler<BulkImportProductsCommand, BulkImportResult>
    {
        private readonly AppDbContext _db;

        public BulkImportProductsHandler(AppDbContext db) => _db = db;

        public async Task<BulkImportResult> Handle(BulkImportProductsCommand cmd, CancellationToken ct)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
            using var reader = new StreamReader(cmd.FilePath);
            using var csv = new CsvReader(reader, config);
            var rows = csv.GetRecords<CsvProductRow>().ToList();

            var categories = await _db.Categories.ToDictionaryAsync(c => c.Name, c => c.Id, ct);
            var seenBarcodes = new HashSet<string>(await _db.Products.Select(p => p.Barcode).ToListAsync(ct));

            var errors = new List<ImportError>();
            var toInsert = new List<Product>();
            int line = 1;

            foreach (var row in rows)
            {
                line++;

                if (string.IsNullOrWhiteSpace(row.Barcode))
                {
                    errors.Add(new ImportError(line, "Thiếu Barcode"));
                    continue;
                }

                if (row.Price <= 0)
                {
                    errors.Add(new ImportError(line, "Giá phải lớn hơn 0"));
                    continue;
                }

                if (!categories.TryGetValue(row.CategoryName, out var categoryId))
                {
                    errors.Add(new ImportError(line, $"Không tìm thấy danh mục '{row.CategoryName}'"));
                    continue;
                }

                if (!seenBarcodes.Add(row.Barcode))
                {
                    errors.Add(new ImportError(line, $"Barcode '{row.Barcode}' đã tồn tại"));
                    continue;
                }

                toInsert.Add(new Product
                {
                    Name = row.Name,
                    Barcode = row.Barcode,
                    Price = row.Price,
                    CostPrice = row.CostPrice,
                    Unit = row.Unit,
                    MinStock = row.MinStock,
                    CurrentStock = row.CurrentStock,
                    CategoryId = categoryId
                });
            }

            if (toInsert.Count > 0)
            {
                _db.Products.AddRange(toInsert);
                await _db.SaveChangesAsync(ct);
            }

            return new BulkImportResult(toInsert.Count, errors.Count, errors);
        }
    }
}

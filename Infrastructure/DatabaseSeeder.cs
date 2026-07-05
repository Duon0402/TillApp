using Microsoft.EntityFrameworkCore;
using TillApp.Models;

namespace TillApp.Infrastructure
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Categories.AnyAsync())
                return;

            var drinks = new Category { Name = "Nước giải khát" };
            var snacks = new Category { Name = "Đồ ăn vặt" };
            db.Categories.AddRange(drinks, snacks);

            db.Products.AddRange(
                new Product
                {
                    Name = "Coca Cola lon",
                    Barcode = "8934588012345",
                    Price = 12000,
                    CostPrice = 8000,
                    Unit = "lon",
                    MinStock = 20,
                    CurrentStock = 100,
                    Category = drinks
                },
                new Product
                {
                    Name = "Pepsi lon",
                    Barcode = "8934588012346",
                    Price = 12000,
                    CostPrice = 8000,
                    Unit = "lon",
                    MinStock = 20,
                    CurrentStock = 80,
                    Category = drinks
                },
                new Product
                {
                    Name = "Snack Oishi",
                    Barcode = "8934588012347",
                    Price = 8000,
                    CostPrice = 5000,
                    Unit = "gói",
                    MinStock = 15,
                    CurrentStock = 50,
                    Category = snacks
                }
            );

            await db.SaveChangesAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TillApp.Models;

namespace TillApp.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<StoreSettings> StoreSettings { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<StockMovement> StockMovements { get; set; } = default!;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyShop.Data.Context
{
    /// <summary>
    /// Factory for creating DbContext instances at design time (for migrations).
    /// Used by EF Core tools when running migrations.
    /// </summary>
    public class MyShopDbContextFactory : IDesignTimeDbContextFactory<MyShopDbContext>
    {
        public MyShopDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyShopDbContext>();

            // Connection string - matches docker-compose.yml
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=myshop;Username=admin;Password=admin123!"
            );

            return new MyShopDbContext(optionsBuilder.Options);
        }
    }
}
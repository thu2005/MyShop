    using Microsoft.EntityFrameworkCore;
    using MyShop.Core.Models;

    namespace MyShop.Data.Context
    {
        public class MyShopDbContext : DbContext
        {
            public MyShopDbContext(DbContextOptions<MyShopDbContext> options)
                : base(options)
            {
            }

            // DbSets
            public DbSet<User> Users { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Apply configurations
                modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyShopDbContext).Assembly);

                // Seed initial data
                SeedData(modelBuilder);
            }

            private void SeedData(ModelBuilder modelBuilder)
            {
                // Seed default admin user
                modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        Id = 1,
                        Username = "admin",
                        // Password: "admin123" - hashed with BCrypt
                        PasswordHash = "$2a$11$XmXPNqzf5IvHZhZk5K5F4.qBz0Y3LV5VqR8hP9xPZGxNzYqH5qG3O",
                        FullName = "Administrator",
                        Email = "admin@myshop.com",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                );

                // Seed categories
                modelBuilder.Entity<Category>().HasData(
                    new Category { Id = 1, Name = "Game console", Description = "Video game consoles, controllers, and gaming accessories" },
                    new Category { Id = 2, Name = "Smartphone", Description = "Latest mobile phones, smartphones, and connectivity devices" },
                    new Category { Id = 3, Name = "Tablet", Description = "Portable tablet computers for productivity and entertainment" },
                    new Category { Id = 4, Name = "Television", Description = "Smart TVs, 4K Ultra HD screens, and home theater systems" }
                );
            }
        }
    }
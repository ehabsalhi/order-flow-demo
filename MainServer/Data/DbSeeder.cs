using MainServer.Entities;
using MainServer.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var passwordHasher = new PasswordHasher<User>();
        var now = DateTime.UtcNow;

        var admin = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@demo.local",
            Role = UserRole.Admin,
            CreatedAt = now,
            UpdatedAt = now,
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");

        var customer = new User
        {
            FirstName = "Customer",
            LastName = "User",
            Email = "customer@demo.local",
            Role = UserRole.Customer,
            CreatedAt = now,
            UpdatedAt = now,
        };
        customer.PasswordHash = passwordHasher.HashPassword(customer, "Customer123!");

        context.Users.AddRange(admin, customer);

        var categories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Electronic devices", CreatedAt = now, UpdatedAt = now },
            new() { Name = "Computers", Description = "Computers and laptops", CreatedAt = now, UpdatedAt = now },
            new() { Name = "Accessories", Description = "Computer and phone accessories", CreatedAt = now, UpdatedAt = now },
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new() { Name = "iPhone", Description = "Apple smartphone", Price = 999.99m, Stock = 50, CategoryId = categories[0].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MacBook", Description = "Apple laptop", Price = 1499.99m, Stock = 30, CategoryId = categories[1].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "AirPods", Description = "Wireless earbuds", Price = 199.99m, Stock = 100, CategoryId = categories[0].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Keyboard", Description = "Mechanical keyboard", Price = 89.99m, Stock = 75, CategoryId = categories[2].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Mouse", Description = "Wireless mouse", Price = 49.99m, Stock = 120, CategoryId = categories[2].Id, CreatedAt = now, UpdatedAt = now },
        };
        context.Products.AddRange(products);

        await context.SaveChangesAsync();
        logger.LogInformation("Development seed data applied successfully.");
    }
}

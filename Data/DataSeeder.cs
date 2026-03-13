using HCL_Food_Delivery.Models;
using HCL_Food_Delivery.Services;
using Microsoft.EntityFrameworkCore;

namespace HCL_Food_Delivery.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FoodDeliveryDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();

        if (!await dbContext.Users.AnyAsync(x => x.Role == UserRole.Admin))
        {
            dbContext.Users.Add(new User
            {
                Name = "Admin",
                Email = "admin@food.local",
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await dbContext.Categories.AnyAsync())
        {
            dbContext.Categories.AddRange(
                new Category { Name = "Pizza" },
                new Category { Name = "Burgers" },
                new Category { Name = "Beverages" });
        }

        await dbContext.SaveChangesAsync();
    }
}


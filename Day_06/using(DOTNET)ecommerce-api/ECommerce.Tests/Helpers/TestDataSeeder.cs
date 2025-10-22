using ECommerce.Domain.Entities;
using ECommerce.Domain.Identity;
using ECommerce.Persistence.Context;

namespace ECommerce.Tests.Helpers;

public static class TestDataSeeder
{
    public static async Task<Category> SeedCategoryAsync(ECommerceDbContext context, string name = "Default Category")
    {
        var category = new Category
        {
            Name = name
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public static async Task<Product> SeedProductAsync(ECommerceDbContext context, Guid categoryId, string name = "Test Product", decimal price = 10.0m, string productCode = "123456")
    {
        var product = new Product
        {
            ProductCode = productCode,
            Name = name,
            Description = "Test Description",
            Price = price,
            ImageUrl = "https://example.com/image.jpg",
            StockQuantity = 100,
            CategoryId = categoryId
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public static async Task SeedMultipleProductsAsync(ECommerceDbContext context, Guid categoryId, int count)
    {
        for (int i = 1; i <= count; i++)
        {
            context.Products.Add(new Product
            {
                ProductCode = $"123{i}",
                Name = $"Product {i}",
                Description = "Bulk Seeded",
                Price = 10 + i,
                ImageUrl = "https://example.com/image.jpg",
                StockQuantity = 100 + i,
                CategoryId = categoryId
            });
        }

        await context.SaveChangesAsync();
    }

    public static async Task<AppUser> SeedUserAsync(ECommerceDbContext context, string username = "testuser", string email = "test@example.com")
    {
        var user = new AppUser
        {
            UserName = username,
            Email = email
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task SeedCartWithItemsAsync(ECommerceDbContext context, Guid userId, Guid categoryId, int itemCount)
    {
        for (int i = 1; i <= itemCount; i++)
        {
            var product = new Product
            {
                ProductCode = $"123{i}",
                Name = $"Product {i}",
                Description = "Cart product",
                Price = 10 * i,
                ImageUrl = "https://example.com/image.jpg",
                StockQuantity = 100,
                CategoryId = categoryId
            };

            context.Products.Add(product);

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                UserId = userId,
                Quantity = (i % 2) + 1 // Random quantity between 1 and 2
            };

            context.CartItems.Add(cartItem);
        }

        await context.SaveChangesAsync();
    }

    public static async Task<Coupon> SeedCouponAsync(ECommerceDbContext context, string code = "TEST10", decimal discount = 10)
    {
        var coupon = new Coupon
        {
            Code = code,
            DiscountAmount = discount,
            ExpiryDate = DateTime.UtcNow.AddMonths(1), // 1 month from now
            IsActive = true
        };

        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();
        return coupon;
    }

    public static async Task<CouponUsage> SeedCouponUsageAsync(ECommerceDbContext context, Guid couponId, Guid userId)
    {
        var couponUsage = new CouponUsage
        {
            CouponId = couponId,
            UserId = userId,
            UsageCount = 1 // Initial usage count
        };
        context.CouponUsages.Add(couponUsage);
        await context.SaveChangesAsync();
        return couponUsage;
    }
}

using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class CartService : ICartService
    {
        private readonly ECommerceDbContext _context;

        public CartService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<List<CartItemResponseDto>> GetCartAsync(Guid userId)
        {
            var items = await _context.CartItems
                .AsNoTracking()
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .Select(ci => new CartItemResponseDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl,
                    ProductPrice = ci.Product.Price,
                    Quantity = ci.Quantity
                })
                .ToListAsync();

            return items;
        }

        public async Task AddOrUpdateCartItemAsync(Guid userId, CartItemDto dto)
        {
            // If the quantity is bigger than stock, adjust it
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                throw new ArgumentException("Product not found");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                // Update existing item
                existingItem.Quantity = dto.Quantity;
                if (existingItem.Quantity > product.StockQuantity)
                    existingItem.Quantity = product.StockQuantity; // Adjust to stock limit
            }
            else
            {
                // Add new item
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                await _context.CartItems.AddAsync(newItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (item == null)
                return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}

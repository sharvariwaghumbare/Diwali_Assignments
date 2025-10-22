using ECommerce.Application.DTOs.Favorite;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ECommerceDbContext _context;

        public FavoriteService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<List<FavoriteDto>> GetFavoritesAsync(Guid userId)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .Select(f => new FavoriteDto
                {
                    ProductId = f.Product.Id,
                    ProductName = f.Product.Name,
                    ProductImage = f.Product.ImageUrl,
                    ProductPrice = f.Product.Price
                })
                .ToListAsync();
        }

        public async Task<FavoriteDto> AddFavoriteAsync(Guid userId, Guid productId)
        {
            var exists = await _context.Favorites.AnyAsync(f =>
                f.UserId == userId && f.ProductId == productId);

            var product = await _context.Products.FindAsync(productId);

            if (!exists)
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();

                return new FavoriteDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImage = product.ImageUrl,
                    ProductPrice = product.Price
                };
            }
            return null;
        }

        public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid productId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (favorite == null)
                return false;

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}

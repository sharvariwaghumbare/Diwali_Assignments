using ECommerce.Application.DTOs.Favorite;

namespace ECommerce.Application.Services.Abstract
{
    public interface IFavoriteService
    {
        Task<List<FavoriteDto>> GetFavoritesAsync(Guid userId);
        Task<FavoriteDto> AddFavoriteAsync(Guid userId, Guid productId);
        Task<bool> RemoveFavoriteAsync(Guid userId, Guid productId);
    }
}

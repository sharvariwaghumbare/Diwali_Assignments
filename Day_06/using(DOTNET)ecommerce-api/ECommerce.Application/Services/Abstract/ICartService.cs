using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Services.Abstract
{
    public interface ICartService
    {
        Task<List<CartItemResponseDto>> GetCartAsync(Guid userId);
        Task AddOrUpdateCartItemAsync(Guid userId, CartItemDto dto);
        Task<bool> RemoveCartItemAsync(Guid userId, Guid cartItemId);
    }
}

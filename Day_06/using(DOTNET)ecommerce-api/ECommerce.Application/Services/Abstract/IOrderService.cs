using ECommerce.Application.DTOs.Order;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services.Abstract
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetMyOrdersAsync(Guid userId);
        Task<OrderDto?> CheckoutAsync(Guid userId, CheckoutRequestDto request);
        // Admin
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<bool> UpdateOrderStatusAsync(Guid id, OrderStatus status);
        Task<bool> CancelOrderAsync(Guid userId, Guid id);

    }
}

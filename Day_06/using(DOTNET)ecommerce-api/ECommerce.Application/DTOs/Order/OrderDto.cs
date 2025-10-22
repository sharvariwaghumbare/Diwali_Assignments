using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}

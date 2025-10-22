using ECommerce.Domain.Enums;
using ECommerce.Domain.Identity;

namespace ECommerce.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }

        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public OrderStatus Status { get; set; }
        public string? CouponCode { get; set; } // Optional

    }

}

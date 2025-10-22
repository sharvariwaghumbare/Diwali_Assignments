using ECommerce.Domain.Identity;

namespace ECommerce.Domain.Entities
{
    public class CouponUsage : BaseEntity
    {
        public Guid CouponId { get; set; }
        public Coupon Coupon { get; set; }

        public Guid UserId { get; set; }
        public AppUser User { get; set; }

        public int UsageCount { get; set; }
    }

}

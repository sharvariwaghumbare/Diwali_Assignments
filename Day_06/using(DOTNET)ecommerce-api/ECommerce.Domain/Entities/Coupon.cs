namespace ECommerce.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int MaxUsageCount { get; set; } = 10; // default 10 per coupon
        public int TotalUsageCount { get; set; } = 0; // total usage by all users
        public int MaxUsagePerUser { get; set; } = 1;
    }

}

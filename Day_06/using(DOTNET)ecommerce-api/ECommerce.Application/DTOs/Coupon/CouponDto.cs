namespace ECommerce.Application.DTOs.Coupon
{
    public class CouponDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? MaxUsageCount { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public int? TotalUsageCount { get; set; }
        public bool IsActive { get; set; }
    }
}

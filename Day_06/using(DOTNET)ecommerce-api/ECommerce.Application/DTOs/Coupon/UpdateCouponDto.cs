namespace ECommerce.Application.DTOs.Coupon
{
    public class UpdateCouponDto
    {
        public string Code { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUsageCount { get; set; }
        public int MaxUsagePerUser { get; set; }
        public bool IsActive { get; set; }
    }
}

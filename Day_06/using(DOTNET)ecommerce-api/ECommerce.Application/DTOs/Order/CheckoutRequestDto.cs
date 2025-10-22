namespace ECommerce.Application.DTOs.Order
{
    public class CheckoutRequestDto
    {
        public string Address { get; set; }
        public string PaymentMethod { get; set; } = "CreditCard"; // Placeholder
        public string? CouponCode { get; set; }
    }
}

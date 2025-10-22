using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services.Concrete;
using ECommerce.Domain.Enums;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class OrderServiceTests : ServiceTestBase
    {
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderService = new OrderService(Context);
        }

        [Fact]
        public async Task CheckoutAsync_ShouldCreateOrder_WhenCartHasItems_AndCouponIsValid()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var user = await TestDataSeeder.SeedUserAsync(Context);
            await TestDataSeeder.SeedCartWithItemsAsync(Context, user.Id, category.Id, itemCount: 2);
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, code: "DISCOUNT20", discount: 20);

            var request = new CheckoutRequestDto
            {
                Address = "123 Test Street",
                CouponCode = "DISCOUNT20",
                PaymentMethod = "PayPal"
            };

            // Act
            var result = await _orderService.CheckoutAsync(user.Id, request);
            var order = await Context.Orders.FirstOrDefaultAsync(o => o.UserId == user.Id && o.Status == OrderStatus.Paid);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().BeGreaterThan(0);
            result.ShippingAddress.Should().Be("123 Test Street");
            result.Status.Should().Be(OrderStatus.Paid);
            result.Items.Should().HaveCount(2);
            order.CouponCode.Should().Be("DISCOUNT20");
            order.TotalAmount.Should().Be(result.TotalAmount);
            order.OrderItems.Should().HaveCount(2);
        }

        [Fact]
        public async Task CheckoutAsync_ShouldReturnNull_WhenCartIsEmpty()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context); // No cart items
            var request = new CheckoutRequestDto
            {
                Address = "Empty Cart Street",
                CouponCode = null,
                PaymentMethod = "PayPal"
            };

            // Act
            var result = await _orderService.CheckoutAsync(user.Id, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckoutAsync_ShouldIgnoreInvalidCoupon_AndStillPlaceOrder()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var user = await TestDataSeeder.SeedUserAsync(Context);
            await TestDataSeeder.SeedCartWithItemsAsync(Context, user.Id, category.Id, itemCount: 2);

            var request = new CheckoutRequestDto
            {
                Address = "Coupon Fail Road",
                CouponCode = "INVALIDCODE", // Does not exist
                PaymentMethod = "PayPal"
            };

            // Act
            var result = await _orderService.CheckoutAsync(user.Id, request);
            var order = await Context.Orders.FirstOrDefaultAsync(o => o.UserId == user.Id && o.Status == OrderStatus.Paid);

            // Assert
            result.Should().NotBeNull();
            order.CouponCode.Should().BeNullOrEmpty();
            result.TotalAmount.Should().BeGreaterThan(0);
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task CheckoutAsync_ShouldCreateOrder_WhenNoCouponIsProvided()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var user = await TestDataSeeder.SeedUserAsync(Context);
            await TestDataSeeder.SeedCartWithItemsAsync(Context, user.Id, category.Id, itemCount: 3);

            var request = new CheckoutRequestDto
            {
                Address = "No Coupon Avenue",
                CouponCode = null, // Explicitly no coupon
                PaymentMethod = "PayPal"
            };

            // Act
            var result = await _orderService.CheckoutAsync(user.Id, request);
            var order = await Context.Orders.FirstOrDefaultAsync(o => o.UserId == user.Id && o.Status == OrderStatus.Paid);

            // Assert
            result.Should().NotBeNull();
            order.CouponCode.Should().BeNull(); // should remain null
            result.TotalAmount.Should().BeGreaterThan(0);
            result.Items.Should().HaveCount(3);
            result.ShippingAddress.Should().Be("No Coupon Avenue");
        }

        [Fact]
        public async Task CheckoutAsync_ShouldIncrementCouponUsage_WhenCouponIsUsed()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var user = await TestDataSeeder.SeedUserAsync(Context);
            await TestDataSeeder.SeedCartWithItemsAsync(Context, user.Id, category.Id, itemCount: 2);
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, code: "USAGECOUPON", discount: 10);
            var request = new CheckoutRequestDto
            {
                Address = "Usage Street",
                CouponCode = "USAGECOUPON",
                PaymentMethod = "PayPal"
            };
            // Act
            var result = await _orderService.CheckoutAsync(user.Id, request);
            var updatedCoupon = await Context.Coupons.FindAsync(coupon.Id);
            var order = await Context.Orders.FirstOrDefaultAsync(o => o.Id == result.Id && o.Status == OrderStatus.Paid);
            // Assert
            updatedCoupon!.TotalUsageCount.Should().Be(1); // Incremented by 1
            order.CouponCode.Should().Be("USAGECOUPON");
            result.TotalAmount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetMyOrdersAsync_ShouldReturnOrdersPlacedByUser()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var user1 = await TestDataSeeder.SeedUserAsync(Context, "user1", "user1@example.com");
            var user2 = await TestDataSeeder.SeedUserAsync(Context, "user2", "user2@example.com");

            await TestDataSeeder.SeedCartWithItemsAsync(Context, user1.Id, category.Id, 2);
            await _orderService.CheckoutAsync(user1.Id, new CheckoutRequestDto
            {
                Address = "User 1 Address",
                PaymentMethod = "Card"
            });

            await TestDataSeeder.SeedCartWithItemsAsync(Context, user2.Id, category.Id, 1);
            await _orderService.CheckoutAsync(user2.Id, new CheckoutRequestDto
            {
                Address = "User 2 Address",
                PaymentMethod = "PayPal"
            });

            // Act
            var result = await _orderService.GetMyOrdersAsync(user1.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().ShippingAddress.Should().Be("User 1 Address");
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldCancelOrder_WhenUserOwnsIt()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var user = await TestDataSeeder.SeedUserAsync(Context);
            await TestDataSeeder.SeedCartWithItemsAsync(Context, user.Id, category.Id, 2);

            var order = await _orderService.CheckoutAsync(user.Id, new CheckoutRequestDto
            {
                Address = "Cancel Me Street",
                PaymentMethod = "Card"
            });

            // Act
            var result = await _orderService.CancelOrderAsync(user.Id, order.Id);

            // Assert
            result.Should().BeTrue();

            var orderInDb = await Context.Orders.FindAsync(order.Id);
            orderInDb!.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenUserDoesNotOwnOrder()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var user1 = await TestDataSeeder.SeedUserAsync(Context);
            var user2 = await TestDataSeeder.SeedUserAsync(Context);

            await TestDataSeeder.SeedCartWithItemsAsync(Context, user1.Id, category.Id, 2);
            var order = await _orderService.CheckoutAsync(user1.Id, new CheckoutRequestDto
            {
                Address = "Wrong User Access",
                PaymentMethod = "Card"
            });

            // Act
            var result = await _orderService.CancelOrderAsync(user2.Id, order.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var randomOrderId = Guid.NewGuid();

            // Act
            var result = await _orderService.CancelOrderAsync(user.Id, randomOrderId);

            // Assert
            result.Should().BeFalse();
        }

    }
}
using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class CartControllerTests
    {
        private readonly CartController _cartController;
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<ICouponService> _couponServiceMock;
        private readonly Guid userId = Guid.NewGuid();

        public CartControllerTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _couponServiceMock = new Mock<ICouponService>();
            var _httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = TestHttpContextFactory.CreateHttpContextWithUserId(userId)
            };
            _cartController = new CartController(_cartServiceMock.Object, _couponServiceMock.Object, _httpContextAccessor);
        }

        [Fact]
        public async Task Get_ShouldReturnCartItems()
        {
            // Arrange
            var cartItems = new List<CartItemResponseDto>
            {
                new CartItemResponseDto { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1 },
                new CartItemResponseDto { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2 }
            };
            _cartServiceMock.Setup(x => x.GetCartAsync(userId)).ReturnsAsync(cartItems);
            // Act
            var result = await _cartController.Get();
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().BeEquivalentTo(ApiResponse<List<CartItemResponseDto>>.SuccessResponse(cartItems));
        }

        [Fact]
        public async Task AddOrUpdate_ShouldAddOrUpdateCartItem()
        {
            // Arrange
            var cartItemDto = new CartItemDto { ProductId = Guid.NewGuid(), Quantity = 1 };
            _cartServiceMock.Setup(x => x.AddOrUpdateCartItemAsync(userId, cartItemDto)).Returns(Task.CompletedTask);
            // Act
            var result = await _cartController.AddOrUpdate(cartItemDto);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().BeEquivalentTo(ApiResponse<string>.SuccessResponse(null, "Item added to cart."));
        }

        [Fact]
        public async Task Remove_ShouldRemoveCartItem()
        {
            // Arrange
            var cartItemId = Guid.NewGuid();
            _cartServiceMock.Setup(x => x.RemoveCartItemAsync(userId, cartItemId)).ReturnsAsync(true);
            // Act
            var result = await _cartController.Remove(cartItemId);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().BeEquivalentTo(ApiResponse<string>.SuccessResponse(null, "Item removed from cart."));
        }

        [Fact]
        public async Task Remove_ShouldThrowBusinessRuleException_WhenItemNotFound()
        {
            // Arrange
            var cartItemId = Guid.NewGuid();
            _cartServiceMock.Setup(x => x.RemoveCartItemAsync(userId, cartItemId)).ReturnsAsync(false);
            // Act
            Func<Task> act = async () => await _cartController.Remove(cartItemId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Item not found in cart.");
        }

        [Fact]
        public async Task ApplyCoupon_ShouldReturnOk_WhenApplied()
        {
            // Arrange
            var couponCode = new ApplyCouponDto
            {
                CouponCode = "SUMMER2025"
            };
            var couponDto = new CouponDto
            {
                Id = Guid.NewGuid(),
                Code = couponCode.CouponCode,
                DiscountAmount = 10.0m,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                MaxUsageCount = 100,
                MaxUsagePerUser = 1,
                TotalUsageCount = 0,
                IsActive = true
            };
            _couponServiceMock.Setup(x => x.ApplyCouponAsync(couponCode.CouponCode, userId)).ReturnsAsync(couponDto);
            // Act
            var result = await _cartController.ApplyCoupon(couponCode);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().BeEquivalentTo(ApiResponse<CouponDto>.SuccessResponse(couponDto, "Coupon applied successfully."));
        }

        [Fact]
        public async Task ApplyCoupon_ShouldReturnNotFound_WhenCouponNotFound()
        {
            // Arrange
            var couponCode = new ApplyCouponDto
            {
                CouponCode = "INVALID"
            };
            _couponServiceMock.Setup(x => x.ApplyCouponAsync(couponCode.CouponCode, Guid.NewGuid())).ReturnsAsync((CouponDto)null);
            // Act
            Func<Task> act = async () => await _cartController.ApplyCoupon(couponCode);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Invalid coupon code.");
        }
    }
}

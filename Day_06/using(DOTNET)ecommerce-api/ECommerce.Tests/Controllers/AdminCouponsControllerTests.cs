using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class AdminCouponsControllerTests
    {
        private readonly Mock<ICouponService> _couponService;
        private readonly AdminCouponsController _controller;

        public AdminCouponsControllerTests()
        {
            _couponService = new Mock<ICouponService>();
            _controller = new AdminCouponsController(_couponService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResult_WithListOfCoupons()
        {
            // Arrange
            var coupons = new List<CouponDto>
            {
                new CouponDto { Id = Guid.NewGuid(), Code = "TEST", DiscountAmount = 10, ExpiryDate = DateTime.UtcNow.AddDays(1), IsActive = true }
            };
            _couponService.Setup(s => s.GetAllAsync()).ReturnsAsync(coupons);
            // Act
            var result = await _controller.GetAll();
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as ApiResponse<List<CouponDto>>;
            response.Should().NotBeNull();
            response.Data.Should().BeEquivalentTo(coupons);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new CouponDto { Id = couponId, Code = "TEST", DiscountAmount = 10, ExpiryDate = DateTime.UtcNow.AddDays(1), IsActive = true };
            _couponService.Setup(s => s.GetByIdAsync(couponId)).ReturnsAsync(coupon);
            // Act
            var result = await _controller.GetById(couponId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as ApiResponse<CouponDto>;
            response.Should().NotBeNull();
            response.Data.Should().Be(coupon);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            _couponService.Setup(s => s.GetByIdAsync(couponId)).ReturnsAsync((CouponDto?)null);
            // Act
            var act = async () => await _controller.GetById(couponId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Coupon not found.");
        }

        [Fact]
        public async Task GetUsage_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var usage = new List<CouponUsage>
            {
                new CouponUsage { Id = Guid.NewGuid(), CouponId = couponId, UserId = Guid.NewGuid(), UsageCount = 0 }
            };
            _couponService.Setup(s => s.GetUsage(couponId)).ReturnsAsync(usage);
            // Act
            var result = await _controller.GetUsage(couponId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as ApiResponse<List<CouponUsage>>;
            response.Should().NotBeNull();
            response.Data.Should().BeEquivalentTo(usage);
        }

        [Fact]
        public async Task GetUsage_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            _couponService.Setup(s => s.GetUsage(couponId)).ReturnsAsync((List<CouponUsage>?)null);
            // Act
            var act = async () => await _controller.GetUsage(couponId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Coupon usage not found.");
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WhenSuccessful()
        {
            // Arrange
            var createDto = new CreateCouponDto { Code = "TEST", DiscountAmount = 10, ExpiryDate = DateTime.UtcNow.AddDays(1) };
            var createdCoupon = new CouponDto { Id = Guid.NewGuid(), Code = "TEST", DiscountAmount = 10, ExpiryDate = DateTime.UtcNow.AddDays(1), IsActive = true };
            _couponService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdCoupon);
            // Act
            var result = await _controller.Create(createDto);
            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            var response = createdResult.Value as ApiResponse<CouponDto>;
            response.Should().NotBeNull();
            response.Data.Should().Be(createdCoupon);
            response.Message.Should().Be("Coupon created.");
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var updateDto = new UpdateCouponDto { Code = "TEST", DiscountAmount = 10, ExpiryDate = DateTime.UtcNow.AddDays(1) };
            _couponService.Setup(s => s.UpdateAsync(couponId, updateDto)).ReturnsAsync(true);
            // Act
            var result = await _controller.Update(couponId, updateDto);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("Coupon updated.");
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var updateDto = new UpdateCouponDto { Code = "TEST", DiscountAmount = 10, ExpiryDate = DateTime.UtcNow.AddDays(1) };
            _couponService.Setup(s => s.UpdateAsync(couponId, updateDto)).ReturnsAsync(false);
            // Act
            var act = async () => await _controller.Update(couponId, updateDto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Coupon not found.");
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            _couponService.Setup(s => s.DeleteAsync(couponId)).ReturnsAsync(true);
            // Act
            var result = await _controller.Delete(couponId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("Coupon deleted.");
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            _couponService.Setup(s => s.DeleteAsync(couponId)).ReturnsAsync(false);
            // Act
            var act = async () => await _controller.Delete(couponId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Coupon not found.");
        }
    }
}

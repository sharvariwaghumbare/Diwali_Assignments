using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Services.Concrete;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class CouponServiceTests : ServiceTestBase
    {
        private readonly CouponService _couponService;

        public CouponServiceTests()
        {
            _couponService = new CouponService(Context);
        }

        [Fact]
        public async Task ApplyCouponAsync_ShouldReturnCoupon_WhenValid()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, code: "SAVE10", discount: 10);
            var user = await TestDataSeeder.SeedUserAsync(Context);

            // Act
            var result = await _couponService.ApplyCouponAsync("SAVE10", user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("SAVE10");
            result.DiscountAmount.Should().Be(10);
        }

        [Fact]
        public async Task ApplyCouponAsync_ShouldReturnNull_WhenCouponIsInactive()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "INACTIVE", 15);
            coupon.IsActive = false;
            Context.Coupons.Update(coupon);
            await Context.SaveChangesAsync();

            // Act
            var result = await _couponService.ApplyCouponAsync("INACTIVE", user.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ApplyCouponAsync_ShouldReturnNull_WhenCouponUsageLimitReached()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "LIMITED", 5);
            coupon.TotalUsageCount = coupon.MaxUsageCount;
            Context.Coupons.Update(coupon);
            await Context.SaveChangesAsync();

            // Act
            var result = await _couponService.ApplyCouponAsync("LIMITED", user.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ApplyCouponAsync_ShouldReturnNull_WhenCouponDoesNotExist()
        {
            // Act
            var result = await _couponService.ApplyCouponAsync("NONEXISTENT", Guid.NewGuid());
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCoupons()
        {
            // Arrange
            await TestDataSeeder.SeedCouponAsync(Context, "COUPON1", 10);
            await TestDataSeeder.SeedCouponAsync(Context, "COUPON2", 20);
            // Act
            var result = await _couponService.GetAllAsync();
            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Code == "COUPON1");
            result.Should().Contain(c => c.Code == "COUPON2");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmpty_WhenNoCouponsExist()
        {
            // Act
            var result = await _couponService.GetAllAsync();
            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCoupon_WhenExists()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "COUPON1", 10);
            // Act
            var result = await _couponService.GetByIdAsync(coupon.Id);
            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("COUPON1");
            result.DiscountAmount.Should().Be(10);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _couponService.GetByIdAsync(Guid.NewGuid());
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUsage_ShouldReturnCouponUsages_WhenExists()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "COUPON1", 10);
            var user = await TestDataSeeder.SeedUserAsync(Context);
            await TestDataSeeder.SeedCouponUsageAsync(Context, coupon.Id, user.Id);
            // Act
            var result = await _couponService.GetUsage(coupon.Id);
            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].CouponId.Should().Be(coupon.Id);
        }

        [Fact]
        public async Task GetUsage_ShouldReturnEmpty_WhenNoUsages()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "COUPON1", 10);
            // Act
            var result = await _couponService.GetUsage(coupon.Id);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateCoupon_WhenValid()
        {
            // Arrange
            var couponDto = new CreateCouponDto
            {
                Code = "NEWCOUPON",
                DiscountAmount = 20,
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            // Act
            var result = await _couponService.CreateAsync(couponDto);
            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be("NEWCOUPON");
            result.DiscountAmount.Should().Be(20);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenInvalidExpiryDate()
        {
            // Arrange
            var couponDto = new CreateCouponDto
            {
                Code = "INVALIDCOUPON",
                DiscountAmount = 10,
                ExpiryDate = DateTime.UtcNow.AddDays(-1) // Expired date
            };
            // Act
            Func<Task> act = async () => await _couponService.CreateAsync(couponDto);
            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("The coupon's expiry date must be set to a future date.");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCouponAlreadyExists()
        {
            // Arrange
            var couponDto = new CreateCouponDto
            {
                Code = "EXISTINGCOUPON",
                DiscountAmount = 10,
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            await _couponService.CreateAsync(couponDto);
            // Act
            Func<Task> act = async () => await _couponService.CreateAsync(couponDto);
            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("A coupon with this code already exists.");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCouponDiscountAmountNotGreaterThanZero()
        {
            // Arrange
            var couponDto = new CreateCouponDto
            {
                Code = "INVALIDDISCOUNT",
                DiscountAmount = 0, // Invalid discount amount
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            // Act
            Func<Task> act = async () => await _couponService.CreateAsync(couponDto);
            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("The discount amount must be greater than zero.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCoupon_WhenValid()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "UPDATECOUPON", 10);
            var updateDto = new UpdateCouponDto
            {
                Code = "UPDATEDCOUPON",
                DiscountAmount = 15,
                ExpiryDate = DateTime.UtcNow.AddMonths(2),
                IsActive = true
            };
            // Act
            var result = await _couponService.UpdateAsync(coupon.Id, updateDto);
            var updatedCoupon = await Context.Coupons.FindAsync(coupon.Id);
            // Assert
            result.Should().BeTrue();
            updatedCoupon.Should().NotBeNull();
            updatedCoupon.Code.Should().Be("UPDATEDCOUPON");
            updatedCoupon.DiscountAmount.Should().Be(15);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenCouponNotFound()
        {
            // Arrange
            var updateDto = new UpdateCouponDto
            {
                Code = "NONEXISTENTCOUPON",
                DiscountAmount = 10,
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            // Act
            var result = await _couponService.UpdateAsync(Guid.NewGuid(), updateDto);
            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenInvalidExpiryDate()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "UPDATECOUPON", 10);
            var updateDto = new UpdateCouponDto
            {
                Code = "INVALIDCOUPON",
                DiscountAmount = 10,
                ExpiryDate = DateTime.UtcNow.AddDays(-1) // Expired date
            };
            // Act
            Func<Task> act = async () => await _couponService.UpdateAsync(coupon.Id, updateDto);
            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("The coupon's expiry date must be set to a future date.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCouponDiscountAmountNotGreaterThanZero()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "UPDATECOUPON", 10);
            var updateDto = new UpdateCouponDto
            {
                Code = "INVALIDDISCOUNT",
                DiscountAmount = 0, // Invalid discount amount
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            // Act
            Func<Task> act = async () => await _couponService.UpdateAsync(coupon.Id, updateDto);
            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("The discount amount must be greater than zero.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCoupon_WhenExists()
        {
            // Arrange
            var coupon = await TestDataSeeder.SeedCouponAsync(Context, "DELETECOUPON", 10);
            // Act
            var result = await _couponService.DeleteAsync(coupon.Id);
            var deletedCoupon = await Context.Coupons.FindAsync(coupon.Id);
            // Assert
            result.Should().BeTrue();
            deletedCoupon!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenCouponNotFound()
        {
            // Act
            var result = await _couponService.DeleteAsync(Guid.NewGuid());
            // Assert
            result.Should().BeFalse();
        }
    }
}
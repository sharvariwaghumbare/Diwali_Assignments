using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class CouponService : ICouponService
    {
        private readonly ECommerceDbContext _context;

        public CouponService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<List<CouponDto>> GetAllAsync()
        {
            return await _context.Coupons
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Select(c => new CouponDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    DiscountAmount = c.DiscountAmount,
                    ExpiryDate = c.ExpiryDate,
                    MaxUsageCount = c.MaxUsageCount,
                    MaxUsagePerUser = c.MaxUsagePerUser,
                    TotalUsageCount = c.TotalUsageCount,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<CouponDto?> GetByIdAsync(Guid id)
        {
            var c = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (c == null) return null;

            return new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                DiscountAmount = c.DiscountAmount,
                ExpiryDate = c.ExpiryDate,
                MaxUsageCount = c.MaxUsageCount,
                MaxUsagePerUser = c.MaxUsagePerUser,
                TotalUsageCount = c.TotalUsageCount,
                IsActive = c.IsActive
            };
        }

        public async Task<List<CouponUsage>> GetUsage(Guid id)
        {
            return await _context.CouponUsages
                .AsNoTracking()
                .Where(cu => cu.CouponId == id)
                .ToListAsync();
        }

        public async Task<CouponDto?> ApplyCouponAsync(string code, Guid userId)
        {
            var coupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted && c.IsActive && c.TotalUsageCount < c.MaxUsageCount);
            if (coupon == null || coupon.ExpiryDate < DateTime.UtcNow) return null;
            // User-specific usage check
            var usage = await _context.CouponUsages
                .FirstOrDefaultAsync(u => u.UserId == userId && u.CouponId == coupon.Id);

            if (usage != null && usage.UsageCount >= coupon.MaxUsagePerUser)
            {
                throw new InvalidOperationException("You have already used this coupon the maximum number of times allowed per user.");
            }
            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountAmount = coupon.DiscountAmount,
                ExpiryDate = coupon.ExpiryDate,
                MaxUsagePerUser = coupon.MaxUsagePerUser,
                IsActive = coupon.IsActive
            };
        }

        public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
        {
            var existingCoupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == dto.Code && !c.IsDeleted);

            if (existingCoupon != null)
            {
                throw new InvalidOperationException("A coupon with this code already exists.");
            }

            if (dto.ExpiryDate < DateTime.UtcNow)
            {
                throw new ArgumentException("The coupon's expiry date must be set to a future date.");
            }

            if (dto.DiscountAmount <= 0)
            {
                throw new ArgumentException("The discount amount must be greater than zero.");
            }

            var coupon = new Coupon
            {
                Code = dto.Code,
                DiscountAmount = dto.DiscountAmount,
                ExpiryDate = dto.ExpiryDate,
                MaxUsageCount = dto.MaxUsageCount ?? 10,
                MaxUsagePerUser = dto.MaxUsagePerUser ?? 1,
                IsActive = true
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountAmount = coupon.DiscountAmount,
                ExpiryDate = coupon.ExpiryDate,
                MaxUsageCount = coupon.MaxUsageCount,
                MaxUsagePerUser = coupon.MaxUsagePerUser,
                IsActive = coupon.IsActive
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCouponDto dto)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (coupon == null) return false;

            if (dto.ExpiryDate < DateTime.UtcNow)
            {
                throw new ArgumentException("The coupon's expiry date must be set to a future date.");
            }

            if (dto.DiscountAmount <= 0)
            {
                throw new ArgumentException("The discount amount must be greater than zero.");
            }

            // Check if the code is being changed and if the new code already exists
            if (!string.Equals(coupon.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
            {
                var existingCoupon = await _context.Coupons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Code == dto.Code && !c.IsDeleted);
                if (existingCoupon != null)
                {
                    throw new InvalidOperationException("A coupon with this code already exists.");
                }
            }

            if (dto.IsActive)
            {
                if (coupon.ExpiryDate < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Cannot activate a coupon that has already expired.");
                }
            }

            coupon.Code = dto.Code;
            coupon.DiscountAmount = dto.DiscountAmount;
            coupon.ExpiryDate = dto.ExpiryDate;
            coupon.MaxUsageCount = dto.MaxUsageCount;
            coupon.MaxUsagePerUser = dto.MaxUsagePerUser;
            coupon.IsActive = dto.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null || coupon.IsDeleted) return false;

            coupon.IsDeleted = true;
            coupon.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}

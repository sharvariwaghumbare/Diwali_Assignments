using ECommerce.Application.DTOs.Coupon;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services.Abstract
{
    public interface ICouponService
    {
        Task<List<CouponDto>> GetAllAsync();
        Task<CouponDto?> GetByIdAsync(Guid id);
        Task<List<CouponUsage>> GetUsage(Guid id);
        Task<CouponDto?> ApplyCouponAsync(string code, Guid userId);
        Task<CouponDto> CreateAsync(CreateCouponDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateCouponDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}

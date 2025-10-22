using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing coupons in admin panel.
    /// </summary>
    /// <response code="403">Forbidden. Only admins can access this endpoint.</response>
    /// <response code="401">Unauthorized. User must be authenticated.</response>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/admin/coupons")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AdminCouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public AdminCouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        /// <summary>
        /// Get all coupons.
        /// </summary>
        /// <returns>A list of coupons</returns>
        /// <response code="200">Returns a list of coupons</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CouponDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var coupons = await _couponService.GetAllAsync();
            return Ok(ApiResponse<List<CouponDto>>.SuccessResponse(coupons));
        }

        /// <summary>
        /// Get a coupon by id.
        /// </summary>
        /// <param name="id">Coupon Id</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/admin/coupons/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Returns the coupon</response>
        /// <response code="400">Coupon not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var coupon = await _couponService.GetByIdAsync(id);
            if (coupon is null)
                throw new BusinessRuleException("Coupon not found.");

            return Ok(ApiResponse<CouponDto>.SuccessResponse(coupon));
        }

        /// <summary>
        /// Get coupon usage by coupon id.
        /// </summary>
        /// <param name="id">Coupon Id</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/admin/coupons/{id}/usage
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Returns the coupon usage</response>
        /// <response code="400">Coupon usage not found</response>
        [HttpGet("{id}/usage")]
        [ProducesResponseType(typeof(ApiResponse<List<CouponUsage>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUsage(Guid id)
        {
            var usage = await _couponService.GetUsage(id);
            if (usage is null)
                throw new BusinessRuleException("Coupon usage not found.");

            return Ok(ApiResponse<List<CouponUsage>>.SuccessResponse(usage));
        }

        /// <summary>
        /// Create a new coupon.
        /// </summary>
        /// <param name="dto">Code, Discount amount, Expiry date</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/admin/coupons
        ///     {
        ///         "Code": "SUMMER2025",
        ///         "DiscountAmount": 20,
        ///         "ExpiryDate": "2025-06-30T23:59:59"
        ///     }
        /// </remarks>
        /// <response code="201">Coupon created</response>
        /// <response code="400">Invalid request</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateCouponDto dto)
        {
            var created = await _couponService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CouponDto>.SuccessResponse(created, "Coupon created."));
        }

        /// <summary>
        /// Update an existing coupon.
        /// </summary>
        /// <param name="id">Coupon Id</param>
        /// <param name="dto">Code, Discount amount, Expiry date</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/admin/coupons/{id}
        ///     {
        ///         "Code": "SUMMER2023",
        ///         "DiscountAmount": 20,
        ///         "ExpiryDate": "2023-12-31T23:59:59"
        ///         "IsActive": true
        ///     }
        /// </remarks>
        /// <response code="200">Coupon updated</response>
        /// <response code="400">Coupon not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, UpdateCouponDto dto)
        {
            var updated = await _couponService.UpdateAsync(id, dto);
            if (!updated)
                throw new BusinessRuleException("Coupon not found.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Coupon updated."));
        }

        /// <summary>
        /// Delete a coupon.
        /// </summary>
        /// <param name="id">Coupon Id</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/admin/coupons/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Coupon deleted</response>
        /// <response code="400">Coupon not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _couponService.DeleteAsync(id);
            if (!deleted)
                throw new BusinessRuleException("Coupon not found.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Coupon deleted."));
        }
    }

}

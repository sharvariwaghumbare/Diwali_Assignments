using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing the shopping cart.
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(ICartService cartService, ICouponService couponService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _couponService = couponService;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        /// <summary>
        /// Gets the current user's shopping cart items.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns the list of cart items.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CartItemResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(ApiResponse<List<CartItemResponseDto>>.SuccessResponse(cart));
        }

        /// <summary>
        /// Adds or updates an item in the user's shopping cart.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///     
        ///     POST /api/cart
        ///     {
        ///         "ProductId": "12345678-1234-1234-1234-123456789012",
        ///         "Quantity": 2
        ///     }
        /// </remarks>
        /// <response code="200">Item added or updated successfully.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddOrUpdate(CartItemDto dto)
        {
            var userId = GetUserId();
            await _cartService.AddOrUpdateCartItemAsync(userId, dto);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Item added to cart."));
        }

        /// <summary>
        /// Removes an item from the user's shopping cart.
        /// </summary>
        /// <param name="cartItemId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/cart/{cartItemId}
        ///     {
        ///         "cartItemId": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Item removed successfully.</response>
        /// <response code="400">Item not found in cart.</response>
        [HttpDelete("{cartItemId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Remove(Guid cartItemId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
            if (!result)
                throw new BusinessRuleException("Item not found in cart.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Item removed from cart."));
        }

        /// <summary>
        /// Applies a coupon code to the user's shopping cart.
        /// </summary>
        /// <param name="code">Coupon Code</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/cart/coupon
        ///     {
        ///         "code": "SUMMER2025"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Coupon applied successfully.</response>
        /// <response code="400">Invalid coupon code.</response>
        [HttpPost("coupon")]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApplyCoupon(ApplyCouponDto dto)
        {
            var userId = GetUserId();
            var coupon = await _couponService.ApplyCouponAsync(dto.CouponCode, userId);
            if (coupon == null)
                throw new BusinessRuleException("Invalid coupon code.");
            return Ok(ApiResponse<CouponDto>.SuccessResponse(coupon, "Coupon applied successfully."));
        }
    }

}

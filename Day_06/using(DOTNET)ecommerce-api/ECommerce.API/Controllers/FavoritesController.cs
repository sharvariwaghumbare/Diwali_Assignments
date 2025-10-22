using System.Security.Claims;
using ECommerce.Application.DTOs.Favorite;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing user favorites (wishlist).
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FavoritesController(IFavoriteService favoriteService, IHttpContextAccessor httpContextAccessor)
        {
            _favoriteService = favoriteService;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        /// <summary>
        /// Gets the current user's favorite products (wishlist).
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns the list of favorite products.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FavoriteDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetUserId();
            var favorites = await _favoriteService.GetFavoritesAsync(userId);
            return Ok(ApiResponse<List<FavoriteDto>>.SuccessResponse(favorites));
        }

        /// <summary>
        /// Adds a product to the user's favorites (wishlist).
        /// </summary>
        /// <param name="dto">ProductID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/favorites
        ///     {
        ///         "ProductId": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <response code="200">Product added to favorites.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FavoriteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddToFavorites(AddFavoriteDto dto)
        {
            var userId = GetUserId();
            var result = await _favoriteService.AddFavoriteAsync(userId, dto.ProductId);
            return Ok(ApiResponse<FavoriteDto>.SuccessResponse(result, "Added to favorites."));
        }

        /// <summary>
        /// Removes a product from the user's favorites (wishlist).
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/favorites/{productId}
        ///     {
        ///         "productId": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Product removed from favorites.</response>
        /// <response code="400">Product not found in favorites.</response>
        [HttpDelete("{productId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveFromFavorites(Guid productId)
        {
            var userId = GetUserId();
            var removed = await _favoriteService.RemoveFavoriteAsync(userId, productId);
            if (!removed)
                throw new BusinessRuleException("Product not found in favorites.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Removed from favorites."));
        }
    }

}

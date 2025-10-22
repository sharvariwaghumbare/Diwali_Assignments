using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing product categories in the application.
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns a list of categories.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
        }

        /// <summary>
        /// Gets a category by its ID.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/categories/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Returns the category details.</response>
        /// <response code="400">Category not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category is null)
                throw new BusinessRuleException("Category not found.");

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
        }

    }

}

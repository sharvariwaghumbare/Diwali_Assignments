using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing categories in the admin panel.
    /// </summary>
    /// <response code="403">Forbidden. Only admins can access this endpoint.</response>
    /// <response code="401">Unauthorized. User must be authenticated.</response>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="dto">Category name</param>
        /// <returns>A response containing the created category.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/admin/categories
        ///     {
        ///         "Name": "Electronics"
        ///     }
        /// </remarks>
        /// <response code="201">Category created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CategoryDto>.SuccessResponse(created, "Category created."));
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">Category Id</param>
        /// <param name="dto">New name for category</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/admin/categories/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012",
        ///         "CategoryName": "Updated Category"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Category updated successfully.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, UpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateAsync(id, dto);
            if (!updated)
                throw new BusinessRuleException("Category not found.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Category updated."));
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="id">Category Id</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/admin/categories/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Category deleted successfully.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _categoryService.DeleteAsync(id);
            if (!deleted)
                throw new BusinessRuleException("Category not found.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Category deleted."));
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

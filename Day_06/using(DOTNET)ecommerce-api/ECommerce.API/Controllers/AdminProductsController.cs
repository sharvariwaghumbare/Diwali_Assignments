using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing products by admin.
    /// </summary>
    /// <response code="403">Forbidden. Only admins can access this endpoint.</response>
    /// <response code="401">Unauthorized. User must be authenticated.</response>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>A response containing the created product.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/admin/products
        ///     {
        ///         "ProductCode": "P001",
        ///         "Name": "Smartphone",
        ///         "Description": "Latest model smartphone",
        ///         "Price": 699.99,
        ///         "ImageUrl": "https://example.com/image.jpg",
        ///         "StockQuantity": 100,
        ///         "CategoryId": "b1a2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"
        ///     }
        /// </remarks>
        /// <response code="201">Product created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var created = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ProductDto>.SuccessResponse(created, "Product created."));
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>
        /// Leave empty fields in the request body to keep the existing values.
        /// Sample request:
        /// 
        ///     PUT /api/admin/products/{id}
        ///     {
        ///         "ProductCode": "P001",
        ///         "Name": "Smartphone Pro",
        ///         "Description": "Latest model smartphone with advanced features",
        ///         "Price": 799.99,
        ///         "ImageUrl": "https://example.com/image_pro.jpg",
        ///         "StockQuantity": 50,
        ///         "CategoryId": "b1a2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Product updated successfully.</response>
        /// <response code="400">Invalid request data or product not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, UpdateProductDto dto)
        {
            var updated = await _productService.UpdateAsync(id, dto);
            if (!updated)
                throw new BusinessRuleException("Product not found.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Product updated."));
        }

        /// <summary>
        /// Deletes a product by ID.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/admin/products/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Product deleted successfully.</response>
        /// <response code="400">Product not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted)
                throw new BusinessRuleException("Product not found.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Product deleted."));
        }

        /// <summary>
        /// Gets all products with optional filtering, sorting, and pagination.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/products
        ///     {
        ///         "minPrice": 100,
        ///         "maxPrice": 1000,
        ///         "sortBy": "Price",
        ///         "pageNumber": 2,
        ///         "pageSize": 5
        ///     }
        /// </remarks>
        /// <response code="200">Returns a paginated list of products.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProductDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
        {
            var products = await _productService.GetProductsAsync(filter);
            return Ok(ApiResponse<PaginatedList<ProductDto>>.SuccessResponse(products));
        }

        /// <summary>
        /// Gets a product by its ID.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/products/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Returns the product details.</response>
        /// <response code="400">Product not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product is null)
                throw new BusinessRuleException("Product not found");

            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }
    }

}

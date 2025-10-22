using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing products in the application.
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
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

using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services.Concrete;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ECommerce.Tests.Controllers;

public class ProductsControllerTests : ServiceTestBase
{
    private readonly ProductService _productService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productService = new ProductService(Context);
        _controller = new ProductsController(_productService);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithProductList()
    {
        // Arrange
        var filter = new ProductFilterDto();
        var category = await TestDataSeeder.SeedCategoryAsync(Context, "Test Category");
        await TestDataSeeder.SeedMultipleProductsAsync(Context, category.Id, 2);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<PaginatedList<ProductDto>>;
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNullOrEmpty();
        response.Data.Items.Count.Should().Be(2);
        response.Data.TotalCount.Should().Be(2);
        response.Data.Page.Should().Be(1);
        response.Data.Items[0].Should().NotBeNull();
        response.Data.Items[0].Name.Should().BeEquivalentTo("Product 1");
        response.Data.Items[1].Should().NotBeNull();
        response.Data.Items[1].Name.Should().BeEquivalentTo("Product 2");
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenNoProductsFound()
    {
        // Arrange
        var filter = new ProductFilterDto();

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as ApiResponse<PaginatedList<ProductDto>>;
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenNoProductsFoundWithFilter()
    {
        // Arrange
        var filter = new ProductFilterDto
        {
            SearchTerm = "Product 3"
        };

        var category = await TestDataSeeder.SeedCategoryAsync(Context, "Test Category");
        await TestDataSeeder.SeedMultipleProductsAsync(Context, category.Id, 2);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as ApiResponse<PaginatedList<ProductDto>>;
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithFilteredProductList_WhenFiltersApplied()
    {
        // Arrange
        var category = await TestDataSeeder.SeedCategoryAsync(Context, "Test Category");
        var filter = new ProductFilterDto
        {
            MinPrice = 12,
            MaxPrice = 14,
            SortBy = "Price",
            SortDirection = "asc"
        };

        await TestDataSeeder.SeedMultipleProductsAsync(Context, category.Id, 10);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as ApiResponse<PaginatedList<ProductDto>>;
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNullOrEmpty();
        response.Data.Items.Count.Should().Be(3);
        response.Data.TotalCount.Should().Be(3);
        response.Data.Page.Should().Be(1);
        response.Data.Items[0].Should().NotBeNull();
        response.Data.Items[0].Name.Should().BeEquivalentTo("Product 2");
        response.Data.Items[1].Should().NotBeNull();
        response.Data.Items[1].Name.Should().BeEquivalentTo("Product 3");
        response.Data.Items[2].Should().NotBeNull();
        response.Data.Items[2].Name.Should().BeEquivalentTo("Product 4");
        response.Data.Items[0].Price.Should().Be(12);
        response.Data.Items[1].Price.Should().Be(13);
        response.Data.Items[2].Price.Should().Be(14);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProductExists()
    {
        // Arrange
        var category = await TestDataSeeder.SeedCategoryAsync(Context, "Test Category");
        var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Test Product", 50);
        // Act
        var result = await _controller.GetById(product.Id);
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as ApiResponse<ProductDto>;
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Name.Should().BeEquivalentTo("Test Product");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        // Act
        var act = async () => await _controller.GetById(productId);
        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Product not found");
    }

}

using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Services.Concrete;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class CategoriesControllerTests : ServiceTestBase
    {
        private readonly CategoryService _categoryService;
        private readonly CategoriesController _categoriesController;

        public CategoriesControllerTests()
        {
            _categoryService = new CategoryService(Context);
            _categoriesController = new CategoriesController(_categoryService);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithCategoryList()
        {
            // Arrange
            await TestDataSeeder.SeedCategoryAsync(Context, "Electronics");
            await TestDataSeeder.SeedCategoryAsync(Context, "Books");
            // Act
            var result = await _categoriesController.GetAll();
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<ApiResponse<List<CategoryDto>>>();
            var response = okResult.Value as ApiResponse<List<CategoryDto>>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Should().HaveCount(2);
            response.Data.Should().Contain(c => c.Name == "Electronics");
            response.Data.Should().Contain(c => c.Name == "Books");
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WithCategory()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context, "Electronics");
            // Act
            var result = await _categoriesController.GetById(category.Id);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<ApiResponse<CategoryDto>>();
            var response = okResult.Value as ApiResponse<CategoryDto>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            // Act
            var act = async () => await _categoriesController.GetById(nonExistentId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Category not found.");
        }
    }
}

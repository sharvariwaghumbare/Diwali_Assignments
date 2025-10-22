using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class AdminCategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly AdminCategoriesController _controller;

        public AdminCategoriesControllerTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _controller = new AdminCategoriesController(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WithCategory()
        {
            // Arrange
            var createDto = new CreateCategoryDto { Name = "Electronics" };
            var createdCategory = new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics" };
            _categoryServiceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdCategory);
            // Act
            var result = await _controller.Create(createDto);
            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Value.Should().BeOfType<ApiResponse<CategoryDto>>();
            var response = createdResult.Value as ApiResponse<CategoryDto>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenCategoryAlreadyExists()
        {
            // Arrange
            var createDto = new CreateCategoryDto { Name = "Electronics" };
            _categoryServiceMock.Setup(s => s.CreateAsync(createDto)).ThrowsAsync(new InvalidOperationException("Category already exists."));
            // Act
            var act = async () => await _controller.Create(createDto);
            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Category already exists.");
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenNameIsNullOrEmpty()
        {
            // Arrange
            var createDto = new CreateCategoryDto { Name = "" };
            _categoryServiceMock.Setup(s => s.CreateAsync(createDto)).ThrowsAsync(new ArgumentException("Category name cannot be empty."));
            // Act
            var act = async () => await _controller.Create(createDto);
            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Category name cannot be empty.");
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenCategoryUpdated()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateDto = new UpdateCategoryDto { Name = "Updated Electronics" };
            _categoryServiceMock.Setup(s => s.UpdateAsync(categoryId, updateDto)).ReturnsAsync(true);
            // Act
            var result = await _controller.Update(categoryId, updateDto);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<ApiResponse<string>>();
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("Category updated.");
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateDto = new UpdateCategoryDto { Name = "Updated Electronics" };
            _categoryServiceMock.Setup(s => s.UpdateAsync(categoryId, updateDto)).ReturnsAsync(false);
            // Act
            var act = async () => await _controller.Update(categoryId, updateDto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Category not found.");
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenCategoryDeleted()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _categoryServiceMock.Setup(s => s.DeleteAsync(categoryId)).ReturnsAsync(true);
            // Act
            var result = await _controller.Delete(categoryId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<ApiResponse<string>>();
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("Category deleted.");
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _categoryServiceMock.Setup(s => s.DeleteAsync(categoryId)).ReturnsAsync(false);
            // Act
            var act = async () => await _controller.Delete(categoryId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Category not found.");
        }
    }
}

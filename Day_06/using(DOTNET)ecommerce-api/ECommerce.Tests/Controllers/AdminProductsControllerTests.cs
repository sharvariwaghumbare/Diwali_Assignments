using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class AdminProductsControllerTests
    {
        private readonly AdminProductsController _controller;
        private readonly Mock<IProductService> _productServiceMock;

        public AdminProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _controller = new AdminProductsController(_productServiceMock.Object);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WhenProductIsCreated()
        {
            // Arrange
            var createDto = new CreateProductDto { Name = "New Product", Price = 100 };
            var createdProduct = new ProductDto { Id = Guid.NewGuid(), Name = "New Product", Price = 100 };
            _productServiceMock.Setup(x => x.CreateAsync(createDto)).ReturnsAsync(createdProduct);
            // Act
            var result = await _controller.Create(createDto);
            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);
            var response = createdResult.Value as ApiResponse<ProductDto>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Name.Should().Be("New Product");
            response.Data.Price.Should().Be(100);
            response.Message.Should().Be("Product created.");
        }

        [Fact]
        public async Task Create_ShouldThrowException_WhenProductAlreadyExists()
        {
            // Arrange
            var createDto = new CreateProductDto { Name = "ExistingProduct", Price = 100 };
            _productServiceMock.Setup(x => x.CreateAsync(createDto)).ThrowsAsync(new Exception("Product with this name already exists"));
            // Act
            var act = async () => await _controller.Create(createDto);
            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Product with this name already exists");
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenProductIsUpdated()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateProductDto { Name = "Updated Product", Price = 150 };
            _productServiceMock.Setup(x => x.UpdateAsync(id, updateDto)).ReturnsAsync(true);
            // Act
            var result = await _controller.Update(id, updateDto);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("Product updated.");
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateProductDto { Name = "Updated Product", Price = 150 };
            _productServiceMock.Setup(x => x.UpdateAsync(id, updateDto)).ReturnsAsync(false);
            // Act
            var act = async () => await _controller.Update(id, updateDto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Product not found.");
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenProductIsDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            _productServiceMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);
            // Act
            var result = await _controller.Delete(id);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("Product deleted.");
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _productServiceMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);
            // Act
            var act = async () => await _controller.Delete(id);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Product not found.");
        }

    }
}

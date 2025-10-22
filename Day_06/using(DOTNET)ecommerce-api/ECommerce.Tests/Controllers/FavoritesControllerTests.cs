using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Favorite;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class FavoritesControllerTests
    {
        private readonly FavoritesController _controller;
        private readonly Mock<IFavoriteService> _favoriteServiceMock;
        private readonly Guid userId = Guid.NewGuid();

        public FavoritesControllerTests()
        {
            _favoriteServiceMock = new Mock<IFavoriteService>();
            var _httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = TestHttpContextFactory.CreateHttpContextWithUserId(userId)
            };
            _controller = new FavoritesController(_favoriteServiceMock.Object, _httpContextAccessor);
        }

        [Fact]
        public async Task GetFavorites_ShouldReturnOk_WithFavoritesList()
        {
            // Arrange
            var favorites = new List<FavoriteDto>
            {
                new FavoriteDto { ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductImage = "Image1.jpg", ProductPrice = 10.0m },
                new FavoriteDto { ProductId = Guid.NewGuid(), ProductName = "Product 2", ProductImage = "Image2.jpg", ProductPrice = 20.0m }
            };
            _favoriteServiceMock.Setup(s => s.GetFavoritesAsync(userId)).ReturnsAsync(favorites);
            // Act
            var result = await _controller.GetFavorites();
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().BeEquivalentTo(ApiResponse<List<FavoriteDto>>.SuccessResponse(favorites));
        }

        [Fact]
        public async Task AddToFavorites_ShouldReturnOk_WhenAddedSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var dto = new AddFavoriteDto { ProductId = productId };
            var favoriteDto = new FavoriteDto { ProductId = productId, ProductName = "Product 1", ProductImage = "Image1.jpg", ProductPrice = 10.0m };
            _favoriteServiceMock.Setup(s => s.AddFavoriteAsync(userId, productId)).ReturnsAsync(favoriteDto);
            // Act
            var result = await _controller.AddToFavorites(dto);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().BeEquivalentTo(ApiResponse<FavoriteDto>.SuccessResponse(favoriteDto, "Added to favorites."));
        }

        [Fact]
        public async Task RemoveFromFavorites_ShouldReturnOk_WhenRemovedSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _favoriteServiceMock.Setup(s => s.RemoveFavoriteAsync(userId, productId)).ReturnsAsync(true);
            // Act
            var result = await _controller.RemoveFromFavorites(productId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().BeEquivalentTo(ApiResponse<string>.SuccessResponse(null, "Removed from favorites."));
        }

        [Fact]
        public async Task RemoveFromFavorites_ShouldThrowBusinessRuleException_WhenProductNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _favoriteServiceMock.Setup(s => s.RemoveFavoriteAsync(userId, productId)).ReturnsAsync(false);
            // Act
            Func<Task> act = async () => await _controller.RemoveFromFavorites(productId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Product not found in favorites.");
        }
    }
}

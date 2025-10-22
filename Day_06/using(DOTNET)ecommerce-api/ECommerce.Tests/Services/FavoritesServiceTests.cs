using ECommerce.Application.Services.Concrete;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class FavoritesServiceTests : ServiceTestBase
    {
        private readonly FavoriteService _favoritesService;

        public FavoritesServiceTests()
        {
            _favoritesService = new FavoriteService(Context);
        }

        [Fact]
        public async Task AddFavoriteAsync_ShouldAddProductToFavorites()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id);

            // Act
            await _favoritesService.AddFavoriteAsync(user.Id, product.Id);

            // Assert
            var favorites = await _favoritesService.GetFavoritesAsync(user.Id);
            favorites.Should().HaveCount(1);
            favorites.First().ProductId.Should().Be(product.Id);
        }

        [Fact]
        public async Task AddFavoriteAsync_ShouldNotDuplicateEntry()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id);

            await _favoritesService.AddFavoriteAsync(user.Id, product.Id);
            await _favoritesService.AddFavoriteAsync(user.Id, product.Id); // Add again

            // Assert
            var favorites = await _favoritesService.GetFavoritesAsync(user.Id);
            favorites.Should().HaveCount(1); // Still one
        }

        [Fact]
        public async Task RemoveFavoriteAsync_ShouldRemoveProduct_WhenExists()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id);

            await _favoritesService.AddFavoriteAsync(user.Id, product.Id);

            // Act
            var result = await _favoritesService.RemoveFavoriteAsync(user.Id, product.Id);

            // Assert
            result.Should().BeTrue();
            var favorites = await _favoritesService.GetFavoritesAsync(user.Id);
            favorites.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveFavoriteAsync_ShouldNotThrow_WhenProductIsNotInFavorites()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var fakeProductId = Guid.NewGuid();

            // Act
            var act = async () => await _favoritesService.RemoveFavoriteAsync(user.Id, fakeProductId);

            // Assert
            await act.Should().NotThrowAsync();
        }

    }
}
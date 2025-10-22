using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Services.Concrete;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class CartServiceTests : ServiceTestBase
    {
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _cartService = new CartService(Context);
        }

        [Fact]
        public async Task AddOrUpdateCartItemAsync_ShouldAddItemToCart()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product A", 25);

            var dto = new CartItemDto
            {
                ProductId = product.Id,
                Quantity = 2
            };

            // Act
            await _cartService.AddOrUpdateCartItemAsync(user.Id, dto);

            // Assert
            var cartItems = await _cartService.GetCartAsync(user.Id);
            cartItems.Should().HaveCount(1);
            cartItems.First().ProductId.Should().Be(product.Id);
            cartItems.First().Quantity.Should().Be(2);
        }

        [Fact]
        public async Task AddOrUpdateCartItemAsync_ShouldUpdateQuantity_IfItemExists()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product A", 25);

            var dto = new CartItemDto
            {
                ProductId = product.Id,
                Quantity = 1
            };

            // Add once
            await _cartService.AddOrUpdateCartItemAsync(user.Id, dto);
            // Add again (should update)
            dto.Quantity = 3;
            await _cartService.AddOrUpdateCartItemAsync(user.Id, dto);

            // Assert
            var cartItems = await _cartService.GetCartAsync(user.Id);
            cartItems.Should().HaveCount(1);
            cartItems.First().Quantity.Should().Be(3);
        }

        [Fact]
        public async Task RemoveCartItemAsync_ShouldRemoveItem_WhenItExists()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Removable Product");

            var dto = new CartItemDto
            {
                ProductId = product.Id,
                Quantity = 1
            };

            await _cartService.AddOrUpdateCartItemAsync(user.Id, dto);

            // Act
            var cart = await _cartService.GetCartAsync(user.Id);
            var result = await _cartService.RemoveCartItemAsync(user.Id, cart.First().Id);

            // Assert
            result.Should().BeTrue();
            cart = await _cartService.GetCartAsync(user.Id);
            cart.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveCartItemAsync_ShouldNotThrow_WhenItemDoesNotExist()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);
            var fakeProductId = Guid.NewGuid();

            // Act
            var act = async () => await _cartService.RemoveCartItemAsync(user.Id, fakeProductId);
            var result = await _cartService.RemoveCartItemAsync(user.Id, fakeProductId);

            // Assert
            await act.Should().NotThrowAsync(); // Silently skips
            result.Should().BeFalse(); // No item to remove
        }

        [Fact]
        public async Task GetCartAsync_ShouldReturnEmptyList_WhenNoItemsExist()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context);

            // Act
            var cart = await _cartService.GetCartAsync(user.Id);

            // Assert
            cart.Should().BeEmpty();
        }

    }
}
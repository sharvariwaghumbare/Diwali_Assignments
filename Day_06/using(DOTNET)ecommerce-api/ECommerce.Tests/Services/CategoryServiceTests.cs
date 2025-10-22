using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Services.Concrete;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class CategoryServiceTests : ServiceTestBase
    {
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _service = new CategoryService(Context);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddCategory_WhenValid()
        {
            // Arrange
            var dto = new CreateCategoryDto { Name = "Books" };

            // Act
            var result = await _service.CreateAsync(dto);
            var inDb = await Context.Categories.FindAsync(result.Id);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Books");
            inDb.Should().NotBeNull();
            inDb!.Name.Should().Be("Books");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            // Arrange
            var dto = new CreateCategoryDto { Name = "" };

            // Act
            var act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Category name cannot be empty.");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCategoryAlreadyExists()
        {
            // Arrange
            var dto = new CreateCategoryDto { Name = "Electronics" };

            // Act
            await _service.CreateAsync(dto);
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Category with name '{dto.Name}' already exists.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenExists()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context, "Old");

            // Act
            var updated = await _service.UpdateAsync(category.Id, new UpdateCategoryDto
            {
                Name = "New"
            });
            var inDb = await Context.Categories.FindAsync(category.Id);

            // Assert
            updated.Should().BeTrue();
            inDb!.Name.Should().Be("New");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryNotFound()
        {
            // Act
            var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateCategoryDto { Name = "Missing" });

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context, "Old");

            // Act
            Func<Task> act = async () => await _service.UpdateAsync(category.Id, new UpdateCategoryDto { Name = "" });

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Category name cannot be empty.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldMarkCategoryAsDeleted()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context, "DeleteMe");

            // Act
            var result = await _service.DeleteAsync(category.Id);
            var inDb = await Context.Categories.FindAsync(category.Id);

            // Assert
            result.Should().BeTrue();
            inDb!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryNotFound()
        {
            // Act
            var result = await _service.DeleteAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyActiveCategories()
        {
            var active = await TestDataSeeder.SeedCategoryAsync(Context, "Visible");
            var deleted = await TestDataSeeder.SeedCategoryAsync(Context, "Hidden");

            deleted.IsDeleted = true;
            Context.Categories.Update(deleted);
            await Context.SaveChangesAsync();

            var result = await _service.GetAllAsync();
            result.Should().ContainSingle(c => c.Name == "Visible");
            result.Should().NotContain(c => c.Name == "Hidden");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context, "FindMe");

            // Act
            var result = await _service.GetByIdAsync(category.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("FindMe");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _service.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
    }

}

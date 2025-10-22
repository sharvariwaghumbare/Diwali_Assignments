using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services.Concrete;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class ProductServiceTests : ServiceTestBase
    {
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _service = new ProductService(Context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoProducts()
        {
            var result = await _service.GetProductsAsync(new ProductFilterDto());

            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_ShouldAddProduct_WhenValid()
        {
            var category = await TestDataSeeder.SeedCategoryAsync(Context);     // Seed 1 Category
            await TestDataSeeder.SeedMultipleProductsAsync(Context, category.Id, 10);  // Seed 10 Products for it

            var dto = new CreateProductDto
            {
                ProductCode = "123456",
                Name = "Test Product",
                Description = "Test Description",
                Price = 49.99m,
                ImageUrl = "https://example.com/image.jpg",
                StockQuantity = 99,
                CategoryId = category.Id
            };

            var created = await _service.CreateAsync(dto);

            var inDb = await Context.Products.FindAsync(created.Id);
            inDb.Should().NotBeNull();
            inDb!.ProductCode.Should().Be("123456");
            inDb!.Name.Should().Be("Test Product");
            inDb.Price.Should().Be(49.99m);
            inDb.CategoryId.Should().Be(category.Id);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            var dto = new CreateProductDto
            {
                ProductCode = "123456",
                Name = "Test Product",
                Description = "Test Description",
                Price = 49.99m,
                ImageUrl = "https://example.com/image.jpg",
                StockQuantity = 99,
                CategoryId = Guid.NewGuid() // Non-existent category
            };
            var result = async () => await _service.CreateAsync(dto);
            await result.Should().ThrowAsync<Exception>().WithMessage("The specified category does not exist.");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenProductCodeAlreadyExists()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var existingProduct = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Existing Product", 10, "123456");
            var dto = new CreateProductDto
            {
                ProductCode = existingProduct.ProductCode,
                Name = "New Product",
                Description = "New Description",
                Price = 29.99m,
                ImageUrl = "https://example.com/new-image.jpg",
                StockQuantity = 50,
                CategoryId = category.Id
            };
            // Act
            var result = async () => await _service.CreateAsync(dto);
            // Assert
            await result.Should().ThrowAsync<Exception>().WithMessage("A product with this code already exists");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyProduct_WhenProductExists()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Old Name", 20);

            var updateDto = new UpdateProductDto
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 99.99m,
                CategoryId = category.Id
            };

            // Act
            var result = await _service.UpdateAsync(product.Id, updateDto);

            // Assert
            result.Should().BeTrue();

            var updatedProduct = await Context.Products.FindAsync(product.Id);
            updatedProduct!.Name.Should().Be("Updated Name");
            updatedProduct.Description.Should().Be("Updated Description");
            updatedProduct.Price.Should().Be(99.99m);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Old Name", 20);

            var updateDto = new UpdateProductDto
            {
                Name = "Should Not Update",
                Description = "Nope",
                Price = 50,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = async () => await _service.UpdateAsync(product.Id, updateDto);

            // Assert
            await result.Should().ThrowAsync<Exception>().WithMessage("The specified category does not exist.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateProductDto
            {
                Name = "Should Not Update",
                Description = "Nope",
                Price = 50,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _service.UpdateAsync(Guid.NewGuid(), updateDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldMarkProductAsDeleted_WhenExists()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Deletable Product");

            // Act
            var result = await _service.DeleteAsync(product.Id);

            // Assert
            result.Should().BeTrue();

            var deletedProduct = await Context.Products.FindAsync(product.Id);
            deletedProduct!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_ShouldNotReturnDeletedProducts()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Visible Product");

            product.IsDeleted = true;
            Context.Products.Update(product);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto());

            // Assert
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            var product = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Special Product");

            // Act
            var result = await _service.GetProductByIdAsync(product.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Special Product");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductNotFound()
        {
            // Arrange
            var randomId = Guid.NewGuid();

            // Act
            var result = await _service.GetProductByIdAsync(randomId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyProductsOfGivenCategory()
        {
            // Arrange
            var category1 = await TestDataSeeder.SeedCategoryAsync(Context, "Category A");
            var category2 = await TestDataSeeder.SeedCategoryAsync(Context, "Category B");

            await TestDataSeeder.SeedProductAsync(Context, category1.Id, "Product A1");
            await TestDataSeeder.SeedProductAsync(Context, category1.Id, "Product A2");
            await TestDataSeeder.SeedProductAsync(Context, category2.Id, "Product B1");

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto { CategoryId = category1.Id });

            // Assert
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(p => p.Name.StartsWith("Product A"));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductsMatchingSearch()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);

            await TestDataSeeder.SeedProductAsync(Context, category.Id, "iPhone 13");
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Samsung S22");
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "iPhone 14");

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto { SearchTerm = "iphone" });

            // Assert
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(p => p.Name.ToLower().Contains("iphone"));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductsWithinPriceRange()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);

            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Cheap Product", price: 10);
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Mid Product", price: 50);
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Expensive Product", price: 100);

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto
            {
                MinPrice = 20,
                MaxPrice = 80
            });

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Mid Product");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductsSortedByPriceAsc()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);

            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product C", price: 30);
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product A", price: 10);
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product B", price: 20);

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto
            {
                SortBy = "price",
                SortDirection = "asc" // can be omitted as default is "asc"
            });

            // Assert
            result.Items.Should().HaveCount(3);
            result.Items[0].Price.Should().Be(10);
            result.Items[1].Price.Should().Be(20);
            result.Items[2].Price.Should().Be(30);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductsSortedByPriceDesc()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);

            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product A", price: 10);
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product B", price: 20);
            await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product C", price: 30);

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto
            {
                SortBy = "price",
                SortDirection = "desc"
            });

            // Assert
            result.Items.Should().HaveCount(3);
            result.Items[0].Price.Should().Be(30);
            result.Items[1].Price.Should().Be(20);
            result.Items[2].Price.Should().Be(10);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductsSortedBySoldDesc()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);

            var productA = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product A");
            var productB = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product B");
            var productC = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product C");

            productA.SoldQuantity = 5;
            productB.SoldQuantity = 20;
            productC.SoldQuantity = 10;

            Context.Products.UpdateRange(productA, productB, productC);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto
            {
                SortBy = "sold",
                SortDirection = "desc"
            });

            // Assert
            result.Items.Should().HaveCount(3);
            result.Items[0].SoldQuantity.Should().Be(20);
            result.Items[1].SoldQuantity.Should().Be(10);
            result.Items[2].SoldQuantity.Should().Be(5);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductsSortedBySoldAsc()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);

            var productA = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product A");
            var productB = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product B");
            var productC = await TestDataSeeder.SeedProductAsync(Context, category.Id, "Product C");

            productA.SoldQuantity = 15;
            productB.SoldQuantity = 5;
            productC.SoldQuantity = 10;

            Context.Products.UpdateRange(productA, productB, productC);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto
            {
                SortBy = "sold",
                SortDirection = "asc" // can be omitted as default is "asc"
            });

            // Assert
            result.Items.Should().HaveCount(3);
            result.Items[0].SoldQuantity.Should().Be(5);
            result.Items[1].SoldQuantity.Should().Be(10);
            result.Items[2].SoldQuantity.Should().Be(15);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnFirstPage_WhenPageNumberIs1()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            await TestDataSeeder.SeedMultipleProductsAsync(Context, category.Id, count: 11); // Seed 11 products

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto
            {
                Page = 1,
                PageSize = 5
            });

            // Assert
            result.Items.Should().HaveCount(5);
            result.Page.Should().Be(1);
            result.TotalCount.Should().Be(11);
            result.TotalPages.Should().Be(3);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnLastPage_WhenPageNumberIs3()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            await TestDataSeeder.SeedMultipleProductsAsync(Context, category.Id, count: 11);

            // Act
            var result = await _service.GetProductsAsync(new ProductFilterDto
            {
                Page = 3,
                PageSize = 5
            });

            // Assert
            result.Items.Should().HaveCount(1);
            result.Page.Should().Be(3);
            result.TotalCount.Should().Be(11);
            result.TotalPages.Should().Be(3);
        }

    }
}
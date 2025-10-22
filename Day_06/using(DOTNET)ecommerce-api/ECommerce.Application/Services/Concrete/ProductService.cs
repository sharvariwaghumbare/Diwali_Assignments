using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class ProductService : IProductService
    {
        private readonly ECommerceDbContext _context;

        public ProductService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(p => p.Name.ToLower().Contains(filter.SearchTerm.ToLower()));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                var sortDirection = string.IsNullOrWhiteSpace(filter.SortDirection) || filter.SortDirection.ToLower() == "asc"
                    ? "ascending" : "descending";
                query = filter.SortBy.ToLower() switch
                {
                    "name" => sortDirection == "ascending" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "price" => sortDirection == "ascending" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                    "sold" => sortDirection == "ascending" ? query.OrderBy(p => p.SoldQuantity) : query.OrderByDescending(p => p.SoldQuantity),
                    _ => query
                };
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    StockQuantity = p.StockQuantity,
                    SoldQuantity = p.SoldQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return new PaginatedList<ProductDto>
            {
                Items = products,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    StockQuantity = p.StockQuantity,
                    SoldQuantity = p.SoldQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ProductCode))
                throw new ArgumentException("Product code cannot be empty.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Product name cannot be empty.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Product description cannot be empty.");

            if (dto.Price <= 0)
                throw new ArgumentException("Product price must be greater than zero.");

            if (string.IsNullOrWhiteSpace(dto.ImageUrl))
                throw new ArgumentException("Product image URL cannot be empty.");

            if (dto.StockQuantity < 0)
                throw new ArgumentException("Product stock quantity cannot be negative.");

            if (dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Category ID cannot be empty.");

            var category = await _context.Categories
                .Where(c => c.Id == dto.CategoryId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (category == null)
                throw new ArgumentException("The specified category does not exist.");

            // Check if the product with the same code already exists
            var isExist = await _context.Products
                .AnyAsync(p => p.ProductCode.ToLower() == dto.ProductCode.ToLower() && !p.IsDeleted);

            if (isExist)
            {
                throw new InvalidOperationException("A product with this code already exists");
            }

            var product = new Product
            {
                ProductCode = dto.ProductCode,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (product == null) return false;

            var category = await _context.Categories
                .Where(c => c.Id == dto.CategoryId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (category == null)
                throw new ArgumentException("The specified category does not exist.");

            // If dto has code, check if the product code already exists
            if (!string.IsNullOrWhiteSpace(dto.ProductCode) &&
                await _context.Products.AnyAsync(p => p.ProductCode.ToLower() == dto.ProductCode.ToLower() && p.Id != id && !p.IsDeleted))
            {
                throw new InvalidOperationException("A product with this code already exists");
            }

            // If dto is null, do not update the properties
            product.ProductCode = dto.ProductCode ?? product.ProductCode;
            product.Name = dto.Name ?? product.Name;
            product.Description = dto.Description ?? product.Description;
            product.Price = dto.Price ?? product.Price;
            product.ImageUrl = dto.ImageUrl ?? product.ImageUrl;
            product.StockQuantity = dto.StockQuantity ?? product.StockQuantity;
            product.CategoryId = dto.CategoryId ?? product.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.IsDeleted) return false;

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

    }

}

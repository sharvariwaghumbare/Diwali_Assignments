using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class CategoryService : ICategoryService
    {
        private readonly ECommerceDbContext _context;

        public CategoryService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return null;

            return new CategoryDto { Id = category.Id, Name = category.Name };
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Category name cannot be empty.");
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name && !c.IsDeleted))
                throw new InvalidOperationException($"Category with name '{dto.Name}' already exists.");

            var category = new Category { Name = dto.Name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto { Id = category.Id, Name = category.Name };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Category name cannot be empty.");

            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
                return false;

            category.Name = dto.Name;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
                return false;

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

    }

}

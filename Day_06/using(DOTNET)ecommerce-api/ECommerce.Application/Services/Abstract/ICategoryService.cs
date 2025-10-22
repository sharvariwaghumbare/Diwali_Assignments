using ECommerce.Application.DTOs.Category;

namespace ECommerce.Application.Services.Abstract
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(Guid id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateCategoryDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}

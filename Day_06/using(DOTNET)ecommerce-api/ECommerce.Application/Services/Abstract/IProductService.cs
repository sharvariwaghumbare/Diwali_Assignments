using ECommerce.Application.DTOs.Product;

namespace ECommerce.Application.Services.Abstract
{
    public interface IProductService
    {
        Task<PaginatedList<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateProductDto dto);
        Task<bool> DeleteAsync(Guid id);

    }

}

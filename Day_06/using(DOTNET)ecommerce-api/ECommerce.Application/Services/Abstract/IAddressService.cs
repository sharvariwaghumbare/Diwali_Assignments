using ECommerce.Application.DTOs.Address;

namespace ECommerce.Application.Services.Abstract
{
    public interface IAddressService
    {
        Task<List<AddressDto>> GetMyAddressesAsync(Guid userId);
        Task<AddressDto?> GetByIdAsync(Guid userId, Guid addressId);
        Task<AddressDto> CreateAsync(Guid userId, CreateAddressDto dto);
        Task<AddressDto> UpdateAsync(Guid userId, Guid addressId, UpdateAddressDto dto);
        Task<bool> DeleteAsync(Guid userId, Guid addressId);
    }
}

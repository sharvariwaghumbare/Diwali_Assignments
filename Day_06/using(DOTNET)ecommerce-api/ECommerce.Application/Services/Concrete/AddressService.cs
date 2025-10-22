using ECommerce.Application.DTOs.Address;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class AddressService : IAddressService
    {
        private readonly ECommerceDbContext _context;

        public AddressService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<List<AddressDto>> GetMyAddressesAsync(Guid userId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    PostalCode = a.PostalCode
                })
                .ToListAsync();
        }

        public async Task<AddressDto?> GetByIdAsync(Guid userId, Guid addressId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.Id == addressId && !a.IsDeleted)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    PostalCode = a.PostalCode
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AddressDto> CreateAsync(Guid userId, CreateAddressDto dto)
        {
            // Title must be unique
            var existingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Title == dto.Title && !a.IsDeleted);
            if (existingAddress != null)
            {
                throw new InvalidOperationException("An address with this title already exists.");
            }

            var address = new Address
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title,
                FullAddress = dto.FullAddress,
                City = dto.City,
                PostalCode = dto.PostalCode
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return new AddressDto
            {
                Id = address.Id,
                Title = address.Title,
                FullAddress = address.FullAddress,
                City = address.City,
                PostalCode = address.PostalCode
            };
        }

        public async Task<AddressDto> UpdateAsync(Guid userID, Guid addressId, UpdateAddressDto addressDto)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userID && a.Id == addressId && !a.IsDeleted);
            if (address == null)
                throw new InvalidOperationException("Address not found.");
            // Check if title is unique
            var existingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userID && a.Title == addressDto.Title && a.Id != addressId && !a.IsDeleted);
            if (existingAddress != null)
            {
                throw new InvalidOperationException("An address with this title already exists.");
            }
            address.Title = addressDto.Title ?? address.Title;
            address.FullAddress = addressDto.FullAddress ?? address.FullAddress;
            address.City = addressDto.City ?? address.City;
            address.PostalCode = addressDto.PostalCode ?? address.PostalCode;
            await _context.SaveChangesAsync();
            return new AddressDto
            {
                Id = address.Id,
                Title = address.Title,
                FullAddress = address.FullAddress,
                City = address.City,
                PostalCode = address.PostalCode
            };
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId && !a.IsDeleted);

            if (address == null)
                return false;

            address.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

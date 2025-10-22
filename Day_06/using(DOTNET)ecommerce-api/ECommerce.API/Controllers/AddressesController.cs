using System.Security.Claims;
using ECommerce.Application.DTOs.Address;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing user addresses.
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        /// <summary>
        /// Gets the current user's addresses.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns the list of addresses.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AddressDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserId();
            var addresses = await _addressService.GetMyAddressesAsync(userId);
            return Ok(ApiResponse<List<AddressDto>>.SuccessResponse(addresses, "Addresses retrieved successfully."));
        }

        /// <summary>
        /// Gets an address by its ID for the current user.
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/addresses/{id}
        ///     {
        ///         "id": "b1a2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"
        ///     }
        /// </remarks>
        /// <response code="200">Returns the address details.</response>
        /// <response code="400">If the address is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetUserId();
            var address = await _addressService.GetByIdAsync(userId, id);
            if (address == null)
                throw new BusinessRuleException("Address not found.");
            return Ok(ApiResponse<AddressDto>.SuccessResponse(address, "Address retrieved successfully."));
        }

        /// <summary>
        /// Creates a new address for the current user.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/addresses
        ///     {
        ///         "Title": "Home",
        ///         "FullAddress": "123 Main St, Springfield",
        ///         "City": "Springfield",
        ///         "PostalCode": "12345"
        ///     }
        /// </remarks>
        /// <response code="201">Address created successfully.</response>
        /// <response code="400">If the address title already exists.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateAddressDto dto)
        {
            var userId = GetUserId();
            var created = await _addressService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<AddressDto>.SuccessResponse(created, "Address created successfully."));
        }

        /// <summary>
        /// Updates an existing address for the current user.
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/addresses/{id}
        ///     {
        ///         "Title": "Work",
        ///         "FullAddress": "456 Elm St, Springfield",
        ///         "City": "Springfield",
        ///         "PostalCode": "67890"
        ///     }
        /// </remarks>
        /// <response code="200">Address updated successfully.</response>
        /// <response code="400">If the address is not found or title already exists.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, UpdateAddressDto dto)
        {
            var userId = GetUserId();
            var updated = await _addressService.UpdateAsync(userId, id, dto);
            return Ok(ApiResponse<AddressDto>.SuccessResponse(updated, "Address updated successfully."));
        }

        /// <summary>
        /// Deletes an address by its ID for the current user.
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <returns></returns>
        /// <response code="200">Address deleted successfully.</response>
        /// <response code="400">If the address is not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            var deleted = await _addressService.DeleteAsync(userId, id);
            if (!deleted)
                throw new BusinessRuleException("Address not found.");
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Address deleted successfully."));
        }
    }
}

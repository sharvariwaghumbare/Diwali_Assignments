using System.Security.Claims;
using ECommerce.Application.DTOs.Profile;
using ECommerce.Application.DTOs.User;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing user profiles.
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        /// <summary>
        /// Updates the user's profile with the provided data.
        /// </summary>
        /// <remarks>This method retrieves the current user's ID and attempts to update their profile
        /// using the provided data. Ensure that <paramref name="dto"/> contains valid and complete profile
        /// information.
        /// Sample request:
        /// 
        ///     PUT /api/profile
        ///     {
        ///         "UserName": "John",
        ///         "Phone": "+1234567890",
        ///     }
        /// </remarks>
        /// <param name="dto">An object containing the updated profile information. This must include valid data for the user's profile.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a 200 OK response with a
        /// success message if the update is successful, or a 400 Bad Request response if the update fails.</returns>
        /// <exception cref="BusinessRuleException">Thrown if the profile update operation fails due to business rule violations.</exception>
        /// <response code="200">Profile updated successfully.</response>
        /// <response code="400">Profile update failed due to invalid data or business rules.</response>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(ProfileUpdateDto dto)
        {
            var userId = GetUserId();
            var success = await _userService.UpdateProfileAsync(userId, dto);
            if (!success)
            {
                throw new BusinessRuleException("Profile update failed.");
            }
            return Ok(ApiResponse<string>.SuccessResponse(null, "Profile updated successfully."));
        }

        /// <summary>
        /// Updates the email address of the currently authenticated user.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated. The new email address provided in 
        /// <paramref name="dto"/> must be valid and meet any applicable business rules.
        /// Sample request:
        /// 
        ///     PUT /api/profile/email
        ///     {
        ///         "NewEmail": "newmail@gmail.com"
        ///     }
        /// </remarks>
        /// <param name="dto">An object containing the new email address to be set.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns a 200 OK response with a
        /// success message if the email change is successful,  or a 400 Bad Request response if the operation fails.</returns>
        /// <exception cref="BusinessRuleException">Thrown if the email change operation fails due to business rule violations.</exception>
        [HttpPut("email")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeEmail(ChangeEmailDto dto)
        {
            var userId = GetUserId();
            var result = await _userService.ChangeEmailAsync(userId, dto.NewEmail);
            if (!result)
            {
                throw new BusinessRuleException("Email change failed.");
            }
            return Ok(ApiResponse<string>.SuccessResponse(null, "Email changed successfully."));
        }

        /// <summary>
        /// Retrieves the current user's information.
        /// </summary>
        /// <returns>User information</returns>
        /// <response code="200">User information retrieved successfully</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new BusinessRuleException("User not found.");
            }
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully."));
        }
    }

}

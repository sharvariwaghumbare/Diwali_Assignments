using ECommerce.Application.DTOs.User;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing users in the admin panel.
    /// </summary>
    /// <response code="403">Forbidden. Only admins can access this endpoint.</response>
    /// <response code="401">Unauthorized. User must be authenticated.</response>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminUsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets all users in the system.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns a list of users.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users));
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/admin/users/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Returns the user details.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user is null)
                throw new BusinessRuleException("User not found.");

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Role name</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/admin/users/{id}/roles
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012",
        ///         "Roles": ["Admin", "User"]
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Role assigned successfully.</response>
        /// <response code="400">Role assign failed. User may not exist.</response>
        [HttpPut("{id}/roles")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRole(Guid id, AssignRoleRequest request)
        {
            var result = await _userService.AssignRoleAsync(id, request.Roles);
            if (!result)
                throw new BusinessRuleException("Role assign failed. User may not exist.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Role assigned."));
        }

        /// <summary>
        /// Bans or unbans a user.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">IsBanned: True(Ban)/False(Unban)</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/admin/users/{id}/ban
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012",
        ///         "IsBanned": true
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">User banned or unbanned successfully.</response>
        /// <response code="400">User not found.</response>
        [HttpPut("{id}/ban")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BanUser(Guid id, BanUserRequest request)
        {
            var result = await _userService.SetUserBanStatusAsync(id, request.IsBanned);
            if (!result)
                throw new BusinessRuleException("User not found.");

            var msg = request.IsBanned ? "User banned." : "User unbanned.";
            return Ok(ApiResponse<string>.SuccessResponse(null, msg));
        }

    }

}

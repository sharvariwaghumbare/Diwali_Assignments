using ECommerce.Application.DTOs.Role;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing roles in the admin panel.
    /// </summary>
    /// <response code="403">Forbidden. Only admins can access this endpoint.</response>
    /// <response code="401">Unauthorized. User must be authenticated.</response>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/admin/roles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AdminRolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public AdminRolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Gets all roles in the system.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns a list of roles.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(ApiResponse<List<RoleDto>>.SuccessResponse(roles));
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="dto">Role Name</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/admin/roles
        ///     {
        ///         "Name": "Manager"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Role created successfully.</response>
        /// <response code="400">Role already exists.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateRoleDto dto)
        {
            var created = await _roleService.CreateRoleAsync(dto.Name);
            if (!created)
                throw new BusinessRuleException("Role already exists.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Role created."));
        }

        /// <summary>
        /// Updates an existing role's name.
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="dto">New name for role</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/admin/roles/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012",
        ///         "NewName": "Updated Role Name"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Role renamed successfully.</response>
        /// <response code="400">Role not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Rename(string id, UpdateRoleDto dto)
        {
            var renamed = await _roleService.RenameRoleAsync(id, dto.NewName);
            if (!renamed)
                throw new BusinessRuleException("Role not found.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Role renamed."));
        }

        /// <summary>
        /// Deletes a role by ID.
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/admin/roles/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Role deleted successfully.</response>
        /// <response code="400">Role not found or could not be deleted.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _roleService.DeleteRoleAsync(id);
            if (!success)
                throw new BusinessRuleException("Role not found or could not be deleted.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Role deleted."));
        }
    }

}

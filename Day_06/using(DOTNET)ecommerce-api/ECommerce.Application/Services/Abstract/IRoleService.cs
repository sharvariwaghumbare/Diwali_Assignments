using ECommerce.Application.DTOs.Role;

namespace ECommerce.Application.Services.Abstract
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<bool> CreateRoleAsync(string name);
        Task<bool> RenameRoleAsync(string roleId, string newName);
        Task<bool> DeleteRoleAsync(string roleId);
    }
}

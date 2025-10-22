using ECommerce.Application.DTOs.Role;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = _roleManager.Roles
                .Where(r => r.Name != "Admin")
                .AsNoTracking()
                .Select(r => new RoleDto
                {
                    Id = r.Id.ToString(),
                    Name = r.Name
                });

            return await roles.ToListAsync();
        }

        public async Task<bool> CreateRoleAsync(string name)
        {
            if (await _roleManager.RoleExistsAsync(name))
                return false;

            var result = await _roleManager.CreateAsync(new AppRole { Name = name });
            return result.Succeeded;
        }

        public async Task<bool> RenameRoleAsync(string roleId, string newName)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return false;

            role.Name = newName;
            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded;
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return false;

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            foreach (var user in usersInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }
    }

}

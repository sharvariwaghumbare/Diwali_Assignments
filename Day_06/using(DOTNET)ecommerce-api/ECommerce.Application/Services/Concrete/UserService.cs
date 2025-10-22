using ECommerce.Application.DTOs.Profile;
using ECommerce.Application.DTOs.User;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services.Concrete
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();

            var result = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    // Skip admin users
                    continue;
                }
                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Username = user.UserName,
                    Roles = roles.ToList(),
                    isBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(1)
                });
            }

            return result;
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Username = user.UserName,
                Roles = roles.ToList(),
                isBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(1)
            };
        }

        public async Task<bool> AssignRoleAsync(Guid userId, string[] role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            foreach (var roleName in role)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    throw new ArgumentException($"Role '{roleName}' does not exist.");
                }
                // Assign the role to the user
                await _userManager.AddToRoleAsync(user, roleName);
            }
            // If user has other roles, remove them
            var currentRoles = await _userManager.GetRolesAsync(user);
            foreach (var currentRole in currentRoles)
            {
                if (!role.Contains(currentRole))
                {
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                }
            }
            return true;
        }

        public async Task<bool> SetUserBanStatusAsync(Guid userId, bool isBanned)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            if (isBanned)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, ProfileUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var updated = false;
            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                user.UserName = dto.Username;
                updated = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                user.PhoneNumber = dto.Phone;
                updated = true;
            }

            if (!updated) return false;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangeEmailAsync(Guid userId, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            if (string.IsNullOrWhiteSpace(newEmail) || newEmail == user.Email)
            {
                return true; // No change needed
            }
            user.Email = newEmail;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

    }

}

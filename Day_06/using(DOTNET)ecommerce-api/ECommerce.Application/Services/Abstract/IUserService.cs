using ECommerce.Application.DTOs.Profile;
using ECommerce.Application.DTOs.User;

namespace ECommerce.Application.Services.Abstract
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<bool> AssignRoleAsync(Guid userId, string[] role);
        Task<bool> SetUserBanStatusAsync(Guid userId, bool isBanned);
        Task<bool> UpdateProfileAsync(Guid userId, ProfileUpdateDto dto);
        Task<bool> ChangeEmailAsync(Guid userId, string newEmail);

    }
}

using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Services.Abstract
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginWithGoogleAsync(string idToken);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Application.Services.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return null; // User already exists

            var existingUsername = await _userManager.FindByNameAsync(request.Username);
            if (existingUsername != null)
                return null; // Username already exists

            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.Username
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return null;

            // Default role: Customer
            if (!await _roleManager.RoleExistsAsync("Customer"))
                await _roleManager.CreateAsync(new AppRole { Name = "Customer" });

            await _userManager.AddToRoleAsync(user, "Customer");

            return await GenerateTokenAsync(user);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return null;

            // If lockout end is in the further future, the user is banned. If lockout end is in near future, the user is temporarily locked out.
            if (user.LockoutEnabled && user.LockoutEnd.HasValue)
            {
                if (user.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(1))
                {
                    // User is banned
                    throw new UnauthorizedAccessException("User is banned.");
                }
                else if (user.LockoutEnd.Value > DateTimeOffset.UtcNow)
                {
                    // User is temporarily locked out
                    throw new UnauthorizedAccessException($"User is locked out until {user.LockoutEnd.Value}.");
                }
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordCheck)
            {
                // If last failed login attempt was more than 1 hour ago, reset access failed count
                if (user.AccessFailedCount > 0 && user.LastFailedLogin < DateTimeOffset.UtcNow.AddHours(-1))
                {
                    await ResetFailedLoginAttemptsAsync(user);
                }
                // Increase access failed count
                await _userManager.AccessFailedAsync(user);
                return null; // Invalid password
            }
            // Reset access failed count and lockout end if the password is correct
            await ResetFailedLoginAttemptsAsync(user);

            return await GenerateTokenAsync(user);
        }

        public async Task<AuthResponse?> LoginWithGoogleAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            });

            if (payload == null) return null;

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = payload.Email.Split('@')[0],
                    Email = payload.Email,
                    EmailConfirmed = true,
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return null;
                await _userManager.AddToRoleAsync(user, "Customer");
            }

            return await GenerateTokenAsync(user);
        }

        private async Task ResetFailedLoginAttemptsAsync(AppUser user)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
            user.LockoutEnd = null; // Clear lockout end if login is successful
            await _userManager.UpdateAsync(user);
        }

        private async Task<AuthResponse> GenerateTokenAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtConfig = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Username = user.UserName,
                Roles = roles
            };
        }
    }

}

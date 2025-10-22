using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Services.Concrete;
using ECommerce.Domain.Identity;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class AuthServiceTests : ServiceTestBase
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<RoleManager<AppRole>> _roleManagerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userManagerMock = TestMockFactory.CreateMockUserManager<AppUser>();
            _roleManagerMock = TestMockFactory.CreateMockRoleManager<AppRole>();
            var _jwtSettings = new Mock<IConfigurationSection>();
            _jwtSettings.Setup(x => x["Key"]).Returns("ThisIsTestJwtSecretKeyDoNotUseInProd123!");
            _jwtSettings.Setup(x => x["Issuer"]).Returns("TestIssuer");
            _jwtSettings.Setup(x => x["Audience"]).Returns("TestAudience");
            _jwtSettings.Setup(x => x["ExpiresInMinutes"]).Returns("60");

            var _configMock = new Mock<IConfiguration>();
            _configMock.Setup(x => x.GetSection("JwtSettings"))
                .Returns(_jwtSettings.Object);
            _configMock.Setup(x => x["Key"]).Returns(_jwtSettings.Object["Key"]);
            _configMock.Setup(x => x["Issuer"]).Returns(_jwtSettings.Object["Issuer"]);
            _configMock.Setup(x => x["Audience"]).Returns(_jwtSettings.Object["Audience"]);
            _configMock.Setup(x => x["ExpiresInMinutes"]).Returns(_jwtSettings.Object["ExpiresInMinutes"]);

            _authService = new AuthService(_userManagerMock.Object, _roleManagerMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnToken_WhenRegistrationSucceeds()
        {
            // Arrange
            var dto = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new AppUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock.Setup(rm => rm.RoleExistsAsync("Customer"))
                .ReturnsAsync(false);

            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<AppRole>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(new List<string> { "Customer" });

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AuthResponse>(result);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.Username, result.Username);
            Assert.Contains("Customer", result.Roles);
            Assert.NotNull(result.Token);
            _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<AppUser>(), dto.Password), Times.Once);
            _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<AppUser>(), "Customer"), Times.Once);
            _roleManagerMock.Verify(rm => rm.RoleExistsAsync("Customer"), Times.Once);
            _roleManagerMock.Verify(rm => rm.CreateAsync(It.IsAny<AppRole>()), Times.Once);
            _userManagerMock.Verify(um => um.GetRolesAsync(It.IsAny<AppUser>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnNull_WhenUserAlreadyExists()
        {
            // Arrange
            var dto = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            var existingUser = new AppUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.Null(result);
            _userManagerMock.Verify(um => um.FindByEmailAsync(dto.Email), Times.Once);
            _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<AppUser>(), dto.Password), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new AppUser
            {
                UserName = "testuser",
                Email = dto.Email,
                LockoutEnd = null
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);

            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Customer" });

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AuthResponse>(result);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(user.UserName, result.Username);
            Assert.Contains("Customer", result.Roles);
            Assert.NotNull(result.Token);
            _userManagerMock.Verify(um => um.FindByEmailAsync(dto.Email), Times.Once);
            _userManagerMock.Verify(um => um.CheckPasswordAsync(user, dto.Password), Times.Once);
            _userManagerMock.Verify(um => um.GetRolesAsync(user), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var dto = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email))
                .ReturnsAsync((AppUser?)null);

            _userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), dto.Password))
                .ReturnsAsync(false);

            _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Null(result);
            _userManagerMock.Verify(um => um.FindByEmailAsync(dto.Email), Times.Once);
            _userManagerMock.Verify(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), dto.Password), Times.Never);
            _userManagerMock.Verify(um => um.GetRolesAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var dto = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new AppUser
            {
                UserName = "testuser",
                Email = dto.Email,
                LockoutEnd = null
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(false);

            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Customer" });

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Null(result);
            _userManagerMock.Verify(um => um.FindByEmailAsync(dto.Email), Times.Once);
            _userManagerMock.Verify(um => um.CheckPasswordAsync(user, dto.Password), Times.Once);
            _userManagerMock.Verify(um => um.GetRolesAsync(user), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenUserIsLockedOut()
        {
            // Arrange
            var dto = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new AppUser
            {
                UserName = "testuser",
                Email = dto.Email,
                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30) // User is locked out
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Customer" });

            // Act
            var act = async () => await _authService.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage($"User is locked out until {user.LockoutEnd.Value}.");
            _userManagerMock.Verify(um => um.FindByEmailAsync(dto.Email), Times.Once);
            _userManagerMock.Verify(um => um.CheckPasswordAsync(user, dto.Password), Times.Never);
            _userManagerMock.Verify(um => um.GetRolesAsync(user), Times.Never);
        }
    }
}

using ECommerce.Application.Services.Concrete;
using ECommerce.Domain.Identity;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class UserServiceTests : ServiceTestBase
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<RoleManager<AppRole>> _roleManagerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userManagerMock = TestMockFactory.CreateMockUserManager<AppUser>();
            _roleManagerMock = TestMockFactory.CreateMockRoleManager<AppRole>();

            _userService = new UserService(_userManagerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task AssignRoleAsync_ShouldAssignRole_WhenUserExists()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context, "john", "john@example.com");

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            _roleManagerMock.Setup(x => x.RoleExistsAsync("Customer"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.AssignRoleAsync(user.Id, ["Customer"]);

            // Assert
            result.Should().BeTrue();

            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Customer"), Times.Once);
        }

        [Fact]
        public async Task AssignRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            _userManagerMock.Setup(x => x.AddToRoleAsync(new AppUser(), "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.AssignRoleAsync(userId, ["Admin"]);

            // Assert
            result.Should().BeFalse();
            _userManagerMock.Verify(x => x.AddToRoleAsync(new AppUser(), "Admin"), Times.Never);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnListOfUsers()
        {
            // Arrange
            var user1 = await TestDataSeeder.SeedUserAsync(Context, "john", "john@example.com");
            var user2 = await TestDataSeeder.SeedUserAsync(Context, "jane", "jane@example.com");
            var user3 = await TestDataSeeder.SeedUserAsync(Context, "jack", "jack@example.com");

            _userManagerMock.Setup(x => x.Users)
                .Returns(new List<AppUser> { user1, user2, user3 }.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(user1))
                .ReturnsAsync(new List<string> { "Admin" }); // Admin is skipped
            _userManagerMock.Setup(x => x.GetRolesAsync(user2))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.GetRolesAsync(user3))
                .ReturnsAsync(new List<string> { "Benutzer" });

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Username.Should().Be("jane");
            result[0].Roles.Should().Contain("User");
            result[1].Username.Should().Be("jack");
            result[1].Roles.Should().Contain("Benutzer");
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            _userManagerMock.Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable());

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context, "john", "john@example.com");

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _userService.GetUserByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            _userManagerMock.Verify(x => x.FindByIdAsync(user.Id.ToString()), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
            _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        }

        [Fact]
        public async Task SetUserBanStatusAsync_ShouldEnableLockout_WhenBanningUser()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context, "john", "john@example.com");
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.SetUserBanStatusAsync(user.Id, true);

            // Assert
            result.Should().BeTrue();
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task SetUserBanStatusAsync_ShouldDisableLockout_WhenUnbanningUser()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUserAsync(Context, "john");

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.SetUserBanStatusAsync(user.Id, false);

            // Assert
            result.Should().BeTrue();
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task SetUserBanStatusAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _userService.SetUserBanStatusAsync(userId, true);

            // Assert
            result.Should().BeFalse();
        }

    }

}

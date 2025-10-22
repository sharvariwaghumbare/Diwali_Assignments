using ECommerce.Application.Services.Concrete;
using ECommerce.Domain.Identity;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services
{
    public class RoleServiceTests
    {
        private readonly Mock<RoleManager<AppRole>> _roleManagerMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly RoleService _service;

        public RoleServiceTests()
        {
            _roleManagerMock = TestMockFactory.CreateMockRoleManager<AppRole>();
            _userManagerMock = TestMockFactory.CreateMockUserManager<AppUser>();
            _service = new RoleService(_roleManagerMock.Object, _userManagerMock.Object);
        }

        //[Fact]
        //public async Task GetAllRolesAsync_ShouldReturnRolesAsDTO_WhenExists()
        //{
        //    // Arrange
        //    var roles = new List<AppRole>
        //    {
        //        new AppRole { Name = "Admin" },
        //        new AppRole { Name = "User" }
        //    };

        //    _roleManagerMock.Setup(x => x.Roles)
        //        .Returns(roles.AsQueryable());

        //    // Act
        //    var result = await _service.GetAllRolesAsync(); // In service, change ToListAsync to ToList for testing

        //    var expected = roles.Select(r => new RoleDto
        //    {
        //        Id = r.Id.ToString(),
        //        Name = r.Name
        //    }).ToList();

        //    // Assert
        //    result.Should().HaveCount(2);
        //    result.Should().BeEquivalentTo(expected);
        //    _roleManagerMock.Verify(x => x.Roles, Times.Once);
        //}

        //[Fact]
        //public async Task GetAllRolesAsync_ShouldReturnEmptyList_WhenNoRolesExist()
        //{
        //    // Arrange
        //    _roleManagerMock.Setup(x => x.Roles)
        //        .Returns(new List<AppRole>().AsQueryable());

        //    // Act
        //    var result = await _service.GetAllRolesAsync(); // In service, change ToListAsync to ToList for testing

        //    // Assert
        //    result.Should().BeEmpty();
        //    _roleManagerMock.Verify(x => x.Roles, Times.Once);
        //}

        [Fact]
        public async Task CreateRoleAsync_ShouldReturnTrue_WhenRoleCreated()
        {
            // Arrange
            _roleManagerMock.Setup(x => x.RoleExistsAsync("Editor"))
                .ReturnsAsync(false);

            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.CreateRoleAsync("Editor");

            // Assert
            result.Should().BeTrue();
            _roleManagerMock.Verify(x => x.RoleExistsAsync("Editor"), Times.Once);
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<AppRole>()), Times.Once);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldReturnFalse_WhenRoleAlreadyExists()
        {
            // Arrange
            _roleManagerMock.Setup(x => x.RoleExistsAsync("Editor"))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateRoleAsync("Editor");

            // Assert
            result.Should().BeFalse();
            _roleManagerMock.Verify(x => x.RoleExistsAsync("Editor"), Times.Once);
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<AppRole>()), Times.Never);
        }

        [Fact]
        public async Task RenameRoleAsync_ShouldUpdateRole_WhenExists()
        {
            // Arrange
            var role = new AppRole { Name = "Old" };

            _roleManagerMock.Setup(x => x.FindByIdAsync(role.Id.ToString()))
                .ReturnsAsync(role);

            _roleManagerMock.Setup(x => x.UpdateAsync(role))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RenameRoleAsync(role.Id.ToString(), "New");

            // Assert
            result.Should().BeTrue();
            role.Name.Should().Be("New");
            _roleManagerMock.Verify(x => x.FindByIdAsync(role.Id.ToString()), Times.Once);
            _roleManagerMock.Verify(x => x.UpdateAsync(role), Times.Once);
        }

        [Fact]
        public async Task RenameRoleAsync_ShouldReturnFalse_WhenRoleNotFound()
        {
            // Arrange
            _roleManagerMock.Setup(x => x.FindByIdAsync("id"))
                .ReturnsAsync((AppRole?)null);

            _roleManagerMock.Setup(x => x.UpdateAsync(It.IsAny<AppRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RenameRoleAsync("id", "New");

            // Assert
            result.Should().BeFalse();
            _roleManagerMock.Verify(x => x.FindByIdAsync("id"), Times.Once);
            _roleManagerMock.Verify(x => x.UpdateAsync(It.IsAny<AppRole>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldReturnTrue_WhenDeleted()
        {
            // Arrange
            var role = new AppRole { Name = "Temp" };

            _userManagerMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
                .ReturnsAsync(new List<AppUser>()); // No users in role

            _roleManagerMock.Setup(x => x.FindByIdAsync(role.Id.ToString()))
                .ReturnsAsync(role);

            _roleManagerMock.Setup(x => x.DeleteAsync(role))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.DeleteRoleAsync(role.Id.ToString());

            // Assert
            result.Should().BeTrue();
            _roleManagerMock.Verify(x => x.FindByIdAsync(role.Id.ToString()), Times.Once);
            _roleManagerMock.Verify(x => x.DeleteAsync(role), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldReturnFalse_WhenRoleNotFound()
        {
            // Arrange
            _roleManagerMock.Setup(x => x.FindByIdAsync("id"))
                .ReturnsAsync((AppRole?)null);
            _roleManagerMock.Setup(x => x.DeleteAsync(It.IsAny<AppRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.DeleteRoleAsync("id");

            // Assert
            result.Should().BeFalse();
            _roleManagerMock.Verify(x => x.FindByIdAsync("id"), Times.Once);
            _roleManagerMock.Verify(x => x.DeleteAsync(It.IsAny<AppRole>()), Times.Never);
        }
    }

}

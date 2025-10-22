using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.User;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class AdminUsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AdminUsersController _controller;

        public AdminUsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new AdminUsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnOk_WithUserList()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = Guid.NewGuid(), Username = "Test" },
                new UserDto { Id = Guid.NewGuid(), Username = "Test2" }
            };
            _userServiceMock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);
            // Act
            var result = await _controller.GetAllUsers();
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<List<UserDto>>;
            response.Should().NotBeNull();
            response.Data.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task GetUser_ShouldReturnOk_WithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserDto { Id = userId, Username = "Test" };
            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(user);
            // Act
            var result = await _controller.GetUser(userId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<UserDto>;
            response.Should().NotBeNull();
            response.Data.Should().BeEquivalentTo(user);
        }

        [Fact]
        public async Task GetUser_ShouldThrowBusinessRuleException_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync((UserDto?)null);
            // Act
            Func<Task> act = async () => await _controller.GetUser(userId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task AssignRole_ShouldReturnOk_WhenRoleAssigned()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new AssignRoleRequest { Roles = ["Admin"] };
            _userServiceMock.Setup(x => x.AssignRoleAsync(userId, request.Roles)).ReturnsAsync(true);
            // Act
            var result = await _controller.AssignRole(userId, request);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("Role assigned.");
        }

        [Fact]
        public async Task AssignRole_ShouldThrowBusinessRuleException_WhenRoleAssignFailed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new AssignRoleRequest { Roles = ["Admin"] };
            _userServiceMock.Setup(x => x.AssignRoleAsync(userId, request.Roles)).ReturnsAsync(false);
            // Act
            Func<Task> act = async () => await _controller.AssignRole(userId, request);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Role assign failed. User may not exist.");
        }

        [Fact]
        public async Task BanUser_ShouldReturnOk_WhenUserBanned()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new BanUserRequest { IsBanned = true };
            _userServiceMock.Setup(x => x.SetUserBanStatusAsync(userId, request.IsBanned)).ReturnsAsync(true);
            // Act
            var result = await _controller.BanUser(userId, request);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Message.Should().Be("User banned.");
        }

        [Fact]
        public async Task BanUser_ShouldThrowBusinessRuleException_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new BanUserRequest { IsBanned = true };
            _userServiceMock.Setup(x => x.SetUserBanStatusAsync(userId, request.IsBanned)).ReturnsAsync(false);
            // Act
            Func<Task> act = async () => await _controller.BanUser(userId, request);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("User not found.");
        }
    }
}

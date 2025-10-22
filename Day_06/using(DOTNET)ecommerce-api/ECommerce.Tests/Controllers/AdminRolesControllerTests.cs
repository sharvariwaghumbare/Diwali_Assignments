using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Role;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class AdminRolesControllerTests
    {
        private readonly AdminRolesController _controller;
        private readonly Mock<IRoleService> _roleServiceMock;

        public AdminRolesControllerTests()
        {
            _roleServiceMock = new Mock<IRoleService>();
            _controller = new AdminRolesController(_roleServiceMock.Object);
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnOkResult_WithListOfRoles()
        {
            // Arrange
            var roles = new List<RoleDto>
            {
                new RoleDto { Id = "1", Name = "Admin" },
                new RoleDto { Id = "2", Name = "User" }
            };
            _roleServiceMock.Setup(s => s.GetAllRolesAsync()).ReturnsAsync(roles);
            // Act
            var result = await _controller.GetAllRoles();
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(ApiResponse<List<RoleDto>>.SuccessResponse(roles));
        }

        [Fact]
        public async Task Create_ShouldReturnOkResult_WhenRoleIsCreated()
        {
            // Arrange
            var createRoleDto = new CreateRoleDto { Name = "Admin" };
            _roleServiceMock.Setup(s => s.CreateRoleAsync(createRoleDto.Name)).ReturnsAsync(true);
            // Act
            var result = await _controller.Create(createRoleDto);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(ApiResponse<string>.SuccessResponse(null, "Role created."));
        }

        [Fact]
        public async Task Create_ShouldThrowBusinessRuleException_WhenRoleAlreadyExists()
        {
            // Arrange
            var createRoleDto = new CreateRoleDto { Name = "Admin" };
            _roleServiceMock.Setup(s => s.CreateRoleAsync(createRoleDto.Name)).ReturnsAsync(false);
            // Act
            Func<Task> act = async () => await _controller.Create(createRoleDto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Role already exists.");
        }

        [Fact]
        public async Task Rename_ShouldReturnOkResult_WhenRoleIsRenamed()
        {
            // Arrange
            var roleId = "1";
            var updateRoleDto = new UpdateRoleDto { NewName = "SuperAdmin" };
            _roleServiceMock.Setup(s => s.RenameRoleAsync(roleId, updateRoleDto.NewName)).ReturnsAsync(true);
            // Act
            var result = await _controller.Rename(roleId, updateRoleDto);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(ApiResponse<string>.SuccessResponse(null, "Role renamed."));
        }

        [Fact]
        public async Task Rename_ShouldThrowBusinessRuleException_WhenRoleNotFound()
        {
            // Arrange
            var roleId = "1";
            var updateRoleDto = new UpdateRoleDto { NewName = "SuperAdmin" };
            _roleServiceMock.Setup(s => s.RenameRoleAsync(roleId, updateRoleDto.NewName)).ReturnsAsync(false);
            // Act
            Func<Task> act = async () => await _controller.Rename(roleId, updateRoleDto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Role not found.");
        }

        [Fact]
        public async Task Delete_ShouldReturnOkResult_WhenRoleIsDeleted()
        {
            // Arrange
            var roleId = "1";
            _roleServiceMock.Setup(s => s.DeleteRoleAsync(roleId)).ReturnsAsync(true);
            // Act
            var result = await _controller.Delete(roleId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(ApiResponse<string>.SuccessResponse(null, "Role deleted."));
        }

        [Fact]
        public async Task Delete_ShouldThrowBusinessRuleException_WhenRoleNotFoundOrCouldNotBeDeleted()
        {
            // Arrange
            var roleId = "1";
            _roleServiceMock.Setup(s => s.DeleteRoleAsync(roleId)).ReturnsAsync(false);
            // Act
            Func<Task> act = async () => await _controller.Delete(roleId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Role not found or could not be deleted.");
        }
    }
}

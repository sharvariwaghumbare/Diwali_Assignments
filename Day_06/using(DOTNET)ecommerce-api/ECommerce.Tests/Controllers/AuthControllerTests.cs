using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class AuthControllerTests : ServiceTestBase
    {
        private readonly AuthController _controller;
        private readonly Mock<IAuthService> _authService;
        private readonly Guid userId = Guid.NewGuid();

        public AuthControllerTests()
        {
            _authService = new Mock<IAuthService>();
            _controller = new AuthController(_authService.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var registerDto = new RegisterRequest
            {
                Username = "testuser",
                Email = "testuser@gmail.com",
                Password = "Password123!"
            };

            var token = Guid.NewGuid();
            var authResponse = new AuthResponse
            {
                Token = token.ToString(),
                Email = registerDto.Email,
                Username = registerDto.Username,
                Roles = new List<string> { "Customer" }
            };
            _authService.Setup(x => x.RegisterAsync(registerDto)).ReturnsAsync(authResponse);
            // Act
            var result = await _controller.Register(registerDto);
            // Assert
            result.Should().NotBeNull();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ApiResponse<AuthResponse>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Token.Should().NotBeNullOrEmpty();
            response.Data.Email.Should().Be(registerDto.Email);
            response.Data.Username.Should().Be(registerDto.Username);
            response.Data.Roles.Should().Contain("Customer");
            response.Message.Should().Be("Registration successful.");
        }

        [Fact]
        public async Task Register_ShouldThrowException_WhenRegistrationFails()
        {
            // Arrange
            var dto = new RegisterRequest
            {
                Username = "testuser",
                Email = "testuser@gmail.com",
                Password = "Password123!"
            };
            _authService.Setup(x => x.RegisterAsync(dto)).ReturnsAsync((AuthResponse)null);
            // Act
            var act = async () => await _controller.Register(dto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Registration failed. Email or username may already be taken.");
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = new LoginRequest
            {
                Email = "testuser@gmail.com",
                Password = "Password123!"
            };
            var token = Guid.NewGuid();
            var authResponse = new AuthResponse
            {
                Token = token.ToString(),
                Email = dto.Email,
                Username = "testuser",
                Roles = new List<string> { "Customer" }
            };
            _authService.Setup(x => x.LoginAsync(dto)).ReturnsAsync(authResponse);
            // Act
            var result = await _controller.Login(dto);
            // Assert
            result.Should().NotBeNull();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ApiResponse<AuthResponse>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Token.Should().NotBeNullOrEmpty();
            response.Data.Email.Should().Be(dto.Email);
            response.Data.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task Login_ShouldThrowException_WhenCredentialsAreInvalid()
        {
            // Arrange
            var dto = new LoginRequest
            {
                Email = "testuser@gmail.com",
                Password = "WrongPassword"
            };
            _authService.Setup(x => x.LoginAsync(dto)).ReturnsAsync((AuthResponse)null);
            // Act
            var act = async () => await _controller.Login(dto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public void Logout_ShouldReturnOk()
        {
            // Act
            var result = _controller.Logout();
            // Assert
            result.Should().NotBeNull();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ApiResponse<string>;
            response.Should().NotBeNull();
            response.Data.Should().BeNull();
            response.Message.Should().Be("Logged out successfully.");
        }
    }
}

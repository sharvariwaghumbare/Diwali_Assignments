using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related actions.
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">Email, username and password</param>
        /// <returns>JWT token</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/register
        ///     {
        ///         "Email": user@gmail.com,
        ///         "Username": "username",
        ///         "Password": "password123"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Invalid request</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result == null)
                throw new BusinessRuleException("Registration failed. Email or username may already be taken.");

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful."));
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        /// <param name="request">Email and password</param>
        /// <returns>JWT token</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///         "Email": user@gmail.com,
        ///         "Password": "password123"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Unauthorized(Banned or locked out)</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
                throw new BusinessRuleException("Invalid email or password.");

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful."));
        }

        /// <summary>
        /// Authenticates a user using their Google account.
        /// </summary>
        /// <remarks>This method processes a Google login request by validating the provided ID token and
        /// returning an authentication response. Ensure that the <paramref name="request"/> contains a valid Google ID
        /// token.</remarks>
        /// <param name="request">The login request containing the Google ID token used for authentication.</param>
        /// <returns>An <see cref="IActionResult"/> containing the authentication response if successful.</returns>
        /// <exception cref="BusinessRuleException">Thrown if the Google login fails, indicating that the provided ID token is invalid or authentication wasunsuccessful.</exception>
        [HttpPost("signin-google")]
        public async Task<IActionResult> SignInWithGoogle([FromBody] GoogleLoginRequest request)
        {
            var result = await _authService.LoginWithGoogleAsync(request.IdToken);

            if (result is null)
                throw new BusinessRuleException("Google login failed.");

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Google login successful."));
        }

        /// <summary>
        /// For JWT, logout is typically handled client-side.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Logout successful</response>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // For JWT, logout is typically handled client-side
            return Ok(ApiResponse<string>.SuccessResponse(null, "Logged out successfully."));
        }

    }

}

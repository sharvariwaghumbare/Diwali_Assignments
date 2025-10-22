using System.Text.Json;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;

namespace ECommerce.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessRuleException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HandleError(context, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await HandleError(context, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HandleError(context, ex.Message);
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HandleError(context, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await HandleError(context, $"An unexpected error occurred. {ex.Message}");
            }
        }

        private static Task HandleError(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            var response = JsonSerializer.Serialize(ApiResponse<string>.Fail(message));
            return context.Response.WriteAsync(response);
        }
    }

}

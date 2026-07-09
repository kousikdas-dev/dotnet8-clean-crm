using System.Net;
using System.Text.Json;
using Crm.Application.Common;
using FluentValidation;

namespace Crm.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ValidationException ex)
            {
                // ⚠️ Validation failures (from FluentValidation)
                _logger.LogWarning("Validation failed: {Errors}", ex.Errors);

                var errorDetails = ex.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList();

                var response = ApiResponse.ErrorResponse(
                    message: "Validation failed",
                    errors: errorDetails,
                    path: context.Request.Path,
                    traceId: context.TraceIdentifier
                );

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                // 🔒 Handle 401 explicitly
                _logger.LogWarning("Unauthorized access: {Message}", ex.Message);

                var response = ApiResponse.ErrorResponse(
                    message: "Unauthorized access",
                    errors: new { ex.Message },
                    path: context.Request.Path,
                    traceId: context.TraceIdentifier
                );

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (KeyNotFoundException ex)
            {
                // ❌ Handle 404 not found
                _logger.LogWarning("Resource not found: {Message}", ex.Message);

                var response = ApiResponse.ErrorResponse(
                    message: "Resource not found",
                    errors: new { ex.Message },
                    path: context.Request.Path,
                    traceId: context.TraceIdentifier
                );

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                // 💥 Unhandled exceptions (fallback)
                _logger.LogError(ex, "Unhandled exception occurred");

                var response = ApiResponse.ErrorResponse(
                    message: "An unexpected error occurred",
                    errors: new { ex.Message, ex.StackTrace },
                    path: context.Request.Path,
                    traceId: context.TraceIdentifier
                );

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

    // ✅ Extension method
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}

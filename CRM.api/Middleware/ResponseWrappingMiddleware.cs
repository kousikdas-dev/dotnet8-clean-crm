using System.Text;
using System.Text.Json;
using Crm.Application.Common;

namespace Crm.Api.Middleware
{
    public class ResponseWrappingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseWrappingMiddleware> _logger;

        public ResponseWrappingMiddleware(RequestDelegate next, ILogger<ResponseWrappingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ Don’t wrap Swagger, Health, or static files
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/favicon.ico"))
            {
                await _next(context);
                return;
            }

            // ✅ Capture original response body
            var originalBodyStream = context.Response.Body;
            await using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await _next(context);

                // ✅ Don't write bodies for status codes that must not have one
                // (avoids: "Writing to the response body is invalid for responses with status code 204.")
                if (context.Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase) ||
                    context.Response.StatusCode is StatusCodes.Status204NoContent or
                        StatusCodes.Status205ResetContent or
                        StatusCodes.Status304NotModified)
                {
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentLength = 0;
                    return;
                }

                // Reset to read response content
                memoryStream.Seek(0, SeekOrigin.Begin);
                var bodyText = await new StreamReader(memoryStream).ReadToEndAsync();

                // ⚠️ Skip wrapping if response already contains "success" property
                if (!string.IsNullOrWhiteSpace(bodyText) &&
                    bodyText.TrimStart().StartsWith("{") &&
                    bodyText.Contains("\"success\"", StringComparison.OrdinalIgnoreCase))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalBodyStream);
                    return;
                }

                // Try to parse as JSON, else keep as raw text
                object? responseData;
                try
                {
                    responseData = JsonSerializer.Deserialize<object>(bodyText);
                }
                catch
                {
                    responseData = bodyText;
                }

                // ✅ Build uniform ApiResponse
                var response = new ApiResponse
                {
                    Success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                    Message = context.Response.StatusCode switch
                    {
                        200 => "Request successful",
                        201 => "Resource created successfully",
                        204 => "No content",
                        400 => "Bad request",
                        401 => "Unauthorized",
                        403 => "Forbidden",
                        404 => "Not found",
                        409 => "Conflict occurred",
                        _ => "Request processed"
                    },
                    Data = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 ? responseData : null,
                    Errors = context.Response.StatusCode >= 400 ? responseData : null,
                    Path = context.Request.Path,
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                };

                context.Response.ContentType = "application/json";
                context.Response.ContentLength = null; // ensure recalculation

                memoryStream.SetLength(0); // Clear old body

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });

                var bytes = Encoding.UTF8.GetBytes(json);
                await originalBodyStream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResponseWrappingMiddleware");

                if (context.Response.HasStarted)
                {
                    // We can't change headers/status or write a new body safely.
                    throw;
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred during response wrapping.",
                    Errors = new { ex.Message },
                    Path = context.Request.Path,
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(errorResponse);
                await originalBodyStream.WriteAsync(Encoding.UTF8.GetBytes(json));
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    public static class ResponseWrappingMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseWrapping(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseWrappingMiddleware>();
        }
    }
}

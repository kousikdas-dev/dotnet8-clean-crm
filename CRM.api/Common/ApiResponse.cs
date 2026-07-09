using System;
using System.Text.Json.Serialization;

namespace Crm.Application.Common
{
    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        [JsonPropertyName("errors")]
        public object? Errors { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // ✅ Factory method for success responses
        public static ApiResponse SuccessResponse(
            string message = "Success",
            object? data = null,
            string? path = null,
            string? traceId = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Data = data,
                Path = path,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        // ✅ Factory method for error responses
        public static ApiResponse ErrorResponse(
            string message = "An error occurred",
            object? errors = null,
            string? path = null,
            string? traceId = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                Path = path,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}

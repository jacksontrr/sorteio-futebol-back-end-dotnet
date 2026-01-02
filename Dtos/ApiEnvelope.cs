using System.Text.Json.Serialization;

namespace Futebol.Api.Dtos
{
    // Standard success envelope: { data: ... }
    public class ApiResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    // Standard error object inside the envelope
    public class ApiError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        // Optional machine-readable error code
        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }

    // Error envelope: { error: { message, code? } }
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public ApiError Error { get; set; } = new ApiError();
    }
}

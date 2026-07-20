namespace HR.WebApi.Models;

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public object? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    public ErrorResponse()
    {
    }

    public ErrorResponse(string message, string errorCode, object? details = null)
    {
        Message = message;
        ErrorCode = errorCode;
        Details = details;
    }
}

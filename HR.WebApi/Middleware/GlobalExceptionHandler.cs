using System.Net;
using System.Text.Json;
using HR.Domain.Exceptions;
using HR.WebApi.Models;

namespace HR.WebApi.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        
        // Log the exception
        _logger.LogError(exception, 
            "An error occurred. TraceId: {TraceId}, Path: {Path}", 
            traceId, 
            context.Request.Path);

        ErrorResponse errorResponse;
        int statusCode;

        switch (exception)
        {
            case BaseException baseEx:
                // Handle custom exceptions
                statusCode = baseEx.StatusCode;
                errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = GetArabicMessage(baseEx),
                    ErrorCode = baseEx.ErrorCode,
                    Details = baseEx.Details,
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId
                };
                break;

            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "غير مصرح لك بالوصول",
                    ErrorCode = "UNAUTHORIZED",
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId
                };
                break;

            case KeyNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "المورد المطلوب غير موجود",
                    ErrorCode = "NOT_FOUND",
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId
                };
                break;

            case FluentValidation.ValidationException valEx:
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "المدخلات المرسلة غير صالحة" ?? "Validation failed",
                    ErrorCode = "VALIDATION_ERROR",
                    Details = valEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => (object)g.Select(e => e.ErrorMessage).ToArray()
                        ),
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId
                };
                break;

            case ArgumentException argEx:
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = argEx.Message,
                    ErrorCode = "INVALID_ARGUMENT",
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId
                };
                break;

            default:
                // Handle unexpected exceptions
                statusCode = StatusCodes.Status500InternalServerError;
                errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = _env.IsDevelopment() 
                        ? exception.Message 
                        : "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Details = _env.IsDevelopment() 
                        ? new { StackTrace = exception.StackTrace, InnerException = exception.InnerException?.Message }
                        : null,
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId
                };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }

    private string GetArabicMessage(BaseException exception)
    {
        // Return the exception message (already in Arabic from our custom exceptions)
        // Or map error codes to Arabic messages
        return exception.Message;
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandler>();
    }
}

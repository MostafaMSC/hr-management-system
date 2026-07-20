namespace HR.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails (400)
/// </summary>
public class ValidationException : BaseException
{
    public ValidationException(string message, object? validationErrors = null)
        : base(
            message: message,
            statusCode: 400,
            errorCode: "VALIDATION_ERROR",
            details: validationErrors)
    {
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base(
            message: "خطأ في البيانات المدخلة",
            statusCode: 400,
            errorCode: "VALIDATION_ERROR",
            details: new { Errors = errors })
    {
    }
}

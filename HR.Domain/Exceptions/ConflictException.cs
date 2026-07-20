namespace HR.Domain.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (409)
/// </summary>
public class ConflictException : BaseException
{
    public ConflictException(string message, object? conflictDetails = null)
        : base(
            message: message,
            statusCode: 409,
            errorCode: "CONFLICT",
            details: conflictDetails)
    {
    }

    public ConflictException(string resourceName, string field, object value)
        : base(
            message: $"{resourceName} مع {field} '{value}' موجود مسبقاً",
            statusCode: 409,
            errorCode: "DUPLICATE_RESOURCE",
            details: new { ResourceName = resourceName, Field = field, Value = value })
    {
    }
}

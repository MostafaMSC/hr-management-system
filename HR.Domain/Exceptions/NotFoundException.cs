namespace HR.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found (404)
/// </summary>
public class NotFoundException : BaseException
{
    public NotFoundException(string resourceName, object resourceId)
        : base(
            message: $"{resourceName} with ID '{resourceId}' was not found",
            statusCode: 404,
            errorCode: $"{resourceName.ToUpper()}_NOT_FOUND",
            details: new { ResourceName = resourceName, ResourceId = resourceId })
    {
    }

    public NotFoundException(string message, string errorCode = "NOT_FOUND")
        : base(
            message: message,
            statusCode: 404,
            errorCode: errorCode)
    {
    }
}

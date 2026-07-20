namespace HR.Domain.Exceptions;

/// <summary>
/// Exception thrown for business logic violations (422)
/// </summary>
public class BusinessException : BaseException
{
    public BusinessException(string message, object? businessDetails = null)
        : base(
            message: message,
            statusCode: 422,
            errorCode: "BUSINESS_RULE_VIOLATION",
            details: businessDetails)
    {
    }

    public BusinessException(string message, string errorCode, object? details = null)
        : base(
            message: message,
            statusCode: 422,
            errorCode: errorCode,
            details: details)
    {
    }
}

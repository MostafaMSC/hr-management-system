namespace HR.Domain.Exceptions;

/// <summary>
/// Base exception class for all custom application exceptions
/// </summary>
public abstract class BaseException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public object? Details { get; }

    protected BaseException(
        string message,
        int statusCode,
        string errorCode,
        object? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }
}

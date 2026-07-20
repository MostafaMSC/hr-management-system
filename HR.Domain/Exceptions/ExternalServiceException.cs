namespace HR.Domain.Exceptions;

/// <summary>
/// Exception thrown when external service call fails (502)
/// </summary>
public class ExternalServiceException : BaseException
{
    public ExternalServiceException(string serviceName, string message, Exception? innerException = null)
        : base(
            message: $"فشل الاتصال بـ {serviceName}: {message}",
            statusCode: 502,
            errorCode: "EXTERNAL_SERVICE_ERROR",
            details: new { ServiceName = serviceName, OriginalMessage = message },
            innerException: innerException)
    {
    }

    public ExternalServiceException(string serviceName, string errorCode, string message)
        : base(
            message: message,
            statusCode: 502,
            errorCode: errorCode,
            details: new { ServiceName = serviceName })
    {
    }
}

namespace HR.Domain.Exceptions;

/// <summary>
/// Exception thrown when authentication fails (401)
/// </summary>
public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "بيانات الدخول غير صحيحة")
        : base(
            message: message,
            statusCode: 401,
            errorCode: "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string message, string errorCode)
        : base(
            message: message,
            statusCode: 401,
            errorCode: errorCode)
    {
    }
}

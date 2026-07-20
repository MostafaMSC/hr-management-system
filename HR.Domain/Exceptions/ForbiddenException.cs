namespace HR.Domain.Exceptions;

/// <summary>
/// Exception thrown when user lacks permission (403)
/// </summary>
public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "ليس لديك صلاحية للوصول إلى هذا المورد")
        : base(
            message: message,
            statusCode: 403,
            errorCode: "FORBIDDEN")
    {
    }

    public ForbiddenException(string resource, string action)
        : base(
            message: $"ليس لديك صلاحية لـ {action} على {resource}",
            statusCode: 403,
            errorCode: "FORBIDDEN",
            details: new { Resource = resource, Action = action })
    {
    }
}

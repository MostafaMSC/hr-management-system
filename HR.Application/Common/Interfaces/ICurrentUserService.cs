using HR.Domain.Enums;

namespace HR.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Username { get; }
        UserType Role { get; }
        int? DepartmentId { get; }
        int? SectionId { get; }
        bool IsAuthenticated { get; }
    }
}

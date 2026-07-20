using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public int UserInfoId { get; set; }
    public UserInfo UserInfo { get; set; } = null!;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public string? Reason { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
}

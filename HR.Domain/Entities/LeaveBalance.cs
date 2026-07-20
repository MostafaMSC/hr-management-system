using HR.Domain.Common;

namespace HR.Domain.Entities;

public class LeaveBalance : BaseEntity
{
    public int UserInfoId { get; set; }
    public UserInfo UserInfo { get; set; } = null!;

    public int Year { get; set; }
    public string LeaveType { get; set; } = string.Empty;

    public int TotalAllowed { get; set; }
    public int Used { get; set; }

    public int Remaining => TotalAllowed - Used;
}

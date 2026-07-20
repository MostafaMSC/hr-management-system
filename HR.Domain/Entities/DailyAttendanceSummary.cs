using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class DailyAttendanceSummary : BaseEntity
{
    public int UserInfoId { get; set; }
    public UserInfo UserInfo { get; set; } = null!;

    public DateTime Date { get; set; }

    public TimeSpan? TimeIn { get; set; }
    public TimeSpan? TimeOut { get; set; }

    public int DelayMinutes { get; set; }
    public int OvertimeMinutes { get; set; }

    public decimal DeductedDays { get; set; }

    public AttendanceStatus Status { get; set; }

    public bool IsDeductionApplied { get; set; }
}

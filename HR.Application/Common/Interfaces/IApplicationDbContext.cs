using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserInfo> UserInfos { get; }
    DbSet<Department> Departments { get; }
    DbSet<Section> Sections { get; }
    DbSet<Device> Devices { get; }
    DbSet<UserDevice> UserDevices { get; }
    DbSet<AttendanceShift> AttendanceShifts { get; }
    DbSet<AttendanceLog> AttendanceLogs { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<LeaveBalance> LeaveBalances { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<DailyAttendanceSummary> DailyAttendanceSummaries { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

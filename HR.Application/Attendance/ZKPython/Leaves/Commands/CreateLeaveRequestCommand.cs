using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Enums;

namespace HR.Application.Attendance.ZKPython.Leaves.Commands
{
    public record CreateLeaveRequestCommand(
        int EmployeeId,
        LeaveType LeaveType,
        LeaveReason LeaveReason,
        string? AdditionalComment,
        int? BackupEmployeeId,
        DateTime? StartDate,
        DateTime? EndDate,
        DateTime? LeaveDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string Reason,
        bool IsManagerRole = false
    ) : IRequest<int>;
}

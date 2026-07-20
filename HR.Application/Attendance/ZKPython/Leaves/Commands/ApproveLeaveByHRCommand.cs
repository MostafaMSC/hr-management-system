using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Leaves.Commands
{
    public record ApproveLeaveByHRCommand(
        int LeaveRequestId,
        int HRUserId,
        string? Comment
    ) : IRequest<bool>;
}

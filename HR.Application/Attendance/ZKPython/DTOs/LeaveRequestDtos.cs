using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Domain.Enums;

namespace HR.Application.Attendance.ZKPython.DTOs
{
    public record CreateLeaveRequestDto(
        LeaveType LeaveType,
        LeaveReason LeaveReason,
        string? AdditionalComment,
        int? BackupEmployeeId,
        DateTime? StartDate,
        DateTime? EndDate,
        DateTime? LeaveDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string Reason
    );

    public record LeaveRequestResponseDto(
        int Id,
        int EmployeeId,
        string EmployeeName,
        string DepartmentName,
        string? SectionName,
        LeaveType LeaveType,
        LeaveReason LeaveReason,
        string? AdditionalComment,
        int? BackupEmployeeId,
        string? BackupEmployeeName,
        LeaveStatus BackupStatus,
        string? BackupComment,
        DateTime? BackupRespondedAt,
        DateTime? StartDate,
        DateTime? EndDate,
        DateTime? LeaveDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string Reason,
        LeaveStatus Status,
        string? ManagerComment,
        string? HRComment,
        string? ApprovedByManagerName,
        string? ApprovedByHRName,
        DateTime? ApprovedByManagerAt,
        DateTime? ApprovedByHRAt,
        string? RejectedByName,
        DateTime? RejectedAt,
        DateTime CreatedAt
    );

    public record ApproveRejectLeaveDto(
        string? Comment
    );

    public record LeaveBalanceDto(
        int EmployeeId,
        int Year,
        int TotalDays,
        int RemainingDays,
        int TotalHours,
        int RemainingHours
    );
}

using System;

namespace HR.Application.Leaves.DTOs;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public int? RequestedShiftId { get; set; }
    public string? LeaveReason { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime? LeaveDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string? ManagerComment { get; set; }
    public string? HrComment { get; set; }
    public string? AttachmentUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

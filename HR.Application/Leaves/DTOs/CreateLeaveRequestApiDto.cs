using System;

namespace HR.Application.Leaves.DTOs;

public class CreateLeaveRequestApiDto
{
    public string Type { get; set; } = string.Empty; // hourly, sick, personal, changeShift, etc.
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? FromTime { get; set; } // HH:mm
    public string? ToTime { get; set; }   // HH:mm

    public string? LeaveReason { get; set; }
    public string Reason { get; set; } = string.Empty;

    public int? RequestedShiftId { get; set; }
}

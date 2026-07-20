using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Domain.Enums;

namespace HR.Application.Attendance.ZKPython.DTOs;

public class CreateTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    // Shared Dynamic Fields
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public int? ManagerId { get; set; }

    // IT Specific Fields
    public string? ComponentName { get; set; }
    public string? HardwareSoftware { get; set; }

    // PKI Specific Fields
    public string? RequestType { get; set; }
    public string? TokenType { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateTicketStatusRequest
{
    public TicketStatus Status { get; set; }
}

public class AssignTicketRequest
{
    public int AssignedToUserId { get; set; }
}

public class AddTicketCommentRequest
{
    public string CommentText { get; set; } = string.Empty;
}

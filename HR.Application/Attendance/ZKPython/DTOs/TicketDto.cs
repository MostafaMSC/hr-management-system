using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System;
using HR.Domain.Enums;
using System.Collections.Generic;

namespace HR.Application.Attendance.ZKPython.DTOs;

public class TicketDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;

    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties for Workflow Handlers
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }

    // SLAs
    public DateTime? DueDate { get; set; }
    public bool IsEscalated { get; set; } = false;

    // Shared Dynamic Fields
    public string? Category { get; set; }
    public string? SubCategory { get; set; }

    // IT Specific Fields
    public string? ComponentName { get; set; }
    public string? HardwareSoftware { get; set; }

    // PKI Specific Fields
    public string? RequestType { get; set; }
    public string? TokenType { get; set; }
    public DateTime? EndDate { get; set; }
}

public class TicketDetailsDto : TicketDto
{
    public List<TicketCommentDto> Comments { get; set; } = new();
    public List<TicketAttachmentDto> Attachments { get; set; } = new();
}

public class TicketCommentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TicketAttachmentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

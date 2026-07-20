using System;
using System.Collections.Generic;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class Ticket : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public int CreatedByUserId { get; set; }
    public UserInfo? CreatedBy { get; set; }

    public int? AssignedToUserId { get; set; }
    public UserInfo? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties for Workflow Handlers
    public int? ManagerId { get; set; }
    public UserInfo? Manager { get; set; }

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

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}

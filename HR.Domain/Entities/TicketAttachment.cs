using System;

namespace HR.Domain.Entities;

public class TicketAttachment : Entity
{
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

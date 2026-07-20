using System;

namespace HR.Domain.Entities;

public class TicketComment : Entity
{
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public int UserId { get; set; }
    public UserInfo? User { get; set; }

    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

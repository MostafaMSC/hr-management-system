using HR.Domain.Common;

namespace HR.Domain.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    
    // If null, it's a global announcement for everyone
    public int? UserId { get; set; }
    public UserInfo? User { get; set; }
}

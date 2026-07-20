using HR.Domain.Common;

namespace HR.Domain.Entities;

public class AttendanceShift : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? LateThreshold { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<UserInfo> Users { get; set; } = new List<UserInfo>();
}

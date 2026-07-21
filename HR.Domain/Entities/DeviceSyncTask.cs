using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class DeviceSyncTask : BaseEntity
{
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public int UserId { get; set; }
    public UserInfo UserInfo { get; set; } = null!;

    public SyncAction Action { get; set; }
    public SyncTaskStatus Status { get; set; } = SyncTaskStatus.Pending;
    
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}

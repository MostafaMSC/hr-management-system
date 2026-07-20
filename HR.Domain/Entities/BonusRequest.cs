using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class BonusRequest : BaseEntity, ISoftDelete
{
    public int RequestingManagerId { get; set; }
    public UserInfo? RequestingManager { get; set; }

    public int TargetUserId { get; set; }
    public UserInfo? TargetUser { get; set; }

    public BonusType Type { get; set; }
    public decimal Value { get; set; }

    public string Reason { get; set; } = string.Empty;

    public BonusStatus Status { get; set; } = BonusStatus.Pending;

    public int? ProcessedByHrId { get; set; }
    public UserInfo? ProcessedByHr { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

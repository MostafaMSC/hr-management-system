using System;
using HR.Domain.Enums;

namespace HR.Application.Bonuses.DTOs;

public class BonusRequestDto
{
    public int Id { get; set; }
    public int RequestingManagerId { get; set; }
    public string RequestingManagerName { get; set; } = string.Empty;
    public int TargetUserId { get; set; }
    public string TargetUserName { get; set; } = string.Empty;
    public BonusType Type { get; set; }
    public decimal Value { get; set; }
    public string Reason { get; set; } = string.Empty;
    public BonusStatus Status { get; set; }
    public int? ProcessedByHrId { get; set; }
    public string? ProcessedByHrName { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime CreatedAt { get; set; }
}

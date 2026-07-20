using System.ComponentModel.DataAnnotations.Schema;
using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public int UserInfoId { get; set; }

    [ForeignKey(nameof(UserInfoId))]
    public UserInfo UserInfo { get; set; } = null!;

    public int? RequestedShiftId { get; set; }
    public AttendanceShift? RequestedShift { get; set; }

    public string? LeaveReason { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime? LeaveDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public LeaveType LeaveType { get; set; } = LeaveType.Personal;
    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    // Manager Approval
    public int? ApprovedByManagerId { get; set; }

    [ForeignKey(nameof(ApprovedByManagerId))]
    public UserInfo? ApprovedByManager { get; set; }
    public string? ManagerComment { get; set; }
    public DateTime? ApprovedByManagerAt { get; set; }

    // Second Line Manager Approval
    public int? ApprovedBySecondLineManagerId { get; set; }

    [ForeignKey(nameof(ApprovedBySecondLineManagerId))]
    public UserInfo? ApprovedBySecondLineManager { get; set; }
    public DateTime? ApprovedBySecondLineManagerAt { get; set; }

    // Attachment
    public string? AttachmentUrl { get; set; }

    // HR Approval
    public int? ApprovedByHRId { get; set; }

    [ForeignKey(nameof(ApprovedByHRId))]
    public UserInfo? ApprovedByHR { get; set; }
    public DateTime? ApprovedByHRAt { get; set; }
    public string? HrComment { get; set; }

    // Rejection tracking
    public int? RejectedById { get; set; }

    [ForeignKey(nameof(RejectedById))]
    public UserInfo? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
}

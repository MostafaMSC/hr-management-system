using System.ComponentModel.DataAnnotations.Schema;
using HR.Domain.Common;

namespace HR.Domain.Entities;

public class UserInfo : BaseEntity, ISoftDelete
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public DateTime DateOfJoining { get; set; }
    
    // Auth & Mobile Additions
    public string? ProfilePictureUrl { get; set; }
    public string? FcmToken { get; set; }
    public bool Is2FAEnabled { get; set; }
    public bool IsSyncedToDevice { get; set; }
    public string? Password { get; set; }
    public string? TwoFactorType { get; set; } // None, Email, Authenticator
    public string? AuthenticatorSecretKey { get; set; }
    public string? CurrentOtp { get; set; }
    public DateTime? OtpExpiry { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Employee ZKTeco Device ID (sometimes called EnrollNumber)
    public string? BiometricId { get; set; } 
    
    // Legacy mapping properties
    public string Username { get; set; } = string.Empty;
    public string? DeviceIp { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Card { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public string? ShiftType { get; set; }
    public string? AccountStatus { get; set; } = "Active";
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }

    public int? AttendanceShiftId { get; set; }
    public AttendanceShift? AttendanceShift { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? SectionId { get; set; }
    public Section? Section { get; set; }

    // Navigation properties
    public ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
    public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    // Extended Profile Information
    public string? FullNameArabic { get; set; }
    public string? MotherName { get; set; }
    public string? PersonalEmail { get; set; }
    public string? PersonalPhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public int? DirectManagerId { get; set; }
    [ForeignKey("DirectManagerId")]
    public UserInfo? DirectManager { get; set; }
    
    public int? ReportToId { get; set; }
    [ForeignKey("ReportToId")]
    public UserInfo? ReportTo { get; set; }
    
    public int? SecondLineManagerId { get; set; }
    [ForeignKey("SecondLineManagerId")]
    public UserInfo? SecondLineManager { get; set; }
    public string? MaritalStatus { get; set; } = "Single";
    public string? City { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? EducationalLevel { get; set; }
    public string? FieldOfStudy { get; set; }
    public string? WorkLocation { get; set; }
    public string? Nationality { get; set; }
    
    // Derived or specific calculated fields (for DTO)
    public int ActualAlreadyTakenLeaves { get; set; }
    public int OvertimeBalanceHours { get; set; }
}

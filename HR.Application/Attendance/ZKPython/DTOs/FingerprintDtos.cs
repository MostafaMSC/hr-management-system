using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
namespace HR.Application.Attendance.ZKPython.DTOs;

/// <summary>
/// DTO Ù„Ø¹Ø±Ø¶ Ù…Ø¹Ù„ÙˆÙ…Ø§Øª Ø§Ù„Ø¨ØµÙ…Ø©
/// </summary>
public class HRDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BiometricId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public int FingerIndex { get; set; }
    public string FingerName { get; set; } = string.Empty;

    /// <summary>
    /// Ø§Ù„Ø¨ØµÙ…Ø© Ø§Ù„ÙØ¹Ù„ÙŠØ© Ù…Ø´ÙØ±Ø© Ø¨ØµÙŠØºØ© Base64
    /// </summary>
    public string? Template { get; set; }

    public int TemplateSize { get; set; }
    public string? DeviceIp { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class PythonTemplateResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int Count { get; set; }
    public List<PythonTemplate> Templates { get; set; } = new();
}

public class PythonTemplate
{
    public int Uid { get; set; }
    public string UserId { get; set; } = string.Empty;  // âœ… This must match "userId" from Python
    public int Fid { get; set; }
    public int Valid { get; set; }
    public string Template { get; set; } = string.Empty;
    public int Size { get; set; }
}

/// <summary>
/// Ù†ØªÙŠØ¬Ø© Ø¹Ù…Ù„ÙŠØ© Ù…Ø²Ø§Ù…Ù†Ø© Ø§Ù„Ø¨ØµÙ…Ø§Øª
/// </summary>
public class SyncHRsResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int Added { get; init; }
    public int Updated { get; init; }
    public int Skipped { get; init; }
    public int Total { get; init; }
    public string? ErrorDetail { get; init; }
}

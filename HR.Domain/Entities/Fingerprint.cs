using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Domain.Entities;

/// <summary>
/// ÙŠÙ…Ø«Ù„ Ø¨ØµÙ…Ø© Ø¥ØµØ¨Ø¹ ÙˆØ§Ø­Ø¯Ø© Ù„Ù…Ø³ØªØ®Ø¯Ù… Ù…Ø¹ÙŠÙ†
/// </summary>
public class Fingerprint : Entity
{
    /// <summary>
    /// Ù…Ø¹Ø±Ù Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø§Ù„Ù…Ø±ØªØ¨Ø· Ø¨Ù‡Ø°Ù‡ Ø§Ù„Ø¨ØµÙ…Ø© (Ù…Ù† Ø§Ù„Ø¬Ù‡Ø§Ø²)
    /// </summary>
    [Required]
    public string DeviceUserId { get; set; } = string.Empty;

    /// <summary>
    /// Ø§Ø³Ù… Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø§Ù„Ù…Ø±ØªØ¨Ø· Ø¨Ù‡Ø°Ù‡ Ø§Ù„Ø¨ØµÙ…Ø©
    /// </summary>
    public string? Username { get; set; }
    public int UserId { get; set; }

    /// <summary>
    /// Ø±Ù‚Ù… Ø§Ù„Ø¥ØµØ¨Ø¹ (0-9)
    /// 0: Ø§Ù„Ø¥Ø¨Ù‡Ø§Ù… Ø§Ù„Ø£ÙŠÙ…Ù†, 1: Ø§Ù„Ø³Ø¨Ø§Ø¨Ø© Ø§Ù„ÙŠÙ…Ù†Ù‰, 2: Ø§Ù„ÙˆØ³Ø·Ù‰ Ø§Ù„ÙŠÙ…Ù†Ù‰, 3: Ø§Ù„Ø¨Ù†ØµØ± Ø§Ù„Ø£ÙŠÙ…Ù†, 4: Ø§Ù„Ø®Ù†ØµØ± Ø§Ù„Ø£ÙŠÙ…Ù†
    /// </summary>
    [Required]
    [Range(0, 9)]
    public int FingerIndex { get; set; }

    /// <summary>
    /// Ø§Ù„Ø¨ØµÙ…Ø© Ø§Ù„ÙØ¹Ù„ÙŠØ© (Template) ÙƒØ¨ÙŠØ§Ù†Ø§Øª Ø«Ù†Ø§Ø¦ÙŠØ©
    /// </summary>
    [Required]
    public byte[] Template { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Ø­Ø¬Ù… Ø§Ù„Ø¨ØµÙ…Ø© Ø¨Ø§Ù„Ø¨Ø§ÙŠØªØ§Øª
    /// </summary>
    [Required]
    public int TemplateSize { get; set; }

    /// <summary>
    /// Ø¹Ù†ÙˆØ§Ù† IP Ù„Ù„Ø¬Ù‡Ø§Ø² Ø§Ù„Ø°ÙŠ ØªÙ… Ø§Ø³ØªØ®Ø±Ø§Ø¬ Ø§Ù„Ø¨ØµÙ…Ø© Ù…Ù†Ù‡
    /// </summary>
    [MaxLength(50)]
    public string? DeviceIp { get; set; }

    /// <summary>
    /// Ù‡Ù„ Ø§Ù„Ø¨ØµÙ…Ø© ØµØ§Ù„Ø­Ø© Ù„Ù„Ø§Ø³ØªØ®Ø¯Ø§Ù…ØŸ
    /// </summary>
    public bool IsValid { get; set; } = true;


    /// <summary>
    /// Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø§Ù„Ù…Ø±ØªØ¨Ø· Ø¨Ù‡Ø°Ù‡ Ø§Ù„Ø¨ØµÙ…Ø©
    /// </summary>
    public virtual UserInfo? User { get; set; }

    /// <summary>
    /// Ø§Ù„Ø­ØµÙˆÙ„ Ø¹Ù„Ù‰ Ø§Ø³Ù… Ø§Ù„Ø¥ØµØ¨Ø¹ Ø¨Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©
    /// </summary>
    [NotMapped]
    public string FingerName => GetFingerName(FingerIndex);

    /// <summary>
    /// ØªØ­ÙˆÙŠÙ„ Ø±Ù‚Ù… Ø§Ù„Ø¥ØµØ¨Ø¹ Ø¥Ù„Ù‰ Ø§Ø³Ù… Ø¨Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©
    /// </summary>
    public static string GetFingerName(int index)
    {
        return index switch
        {
            0 => "Ø§Ù„Ø¥Ø¨Ù‡Ø§Ù… Ø§Ù„Ø£ÙŠÙ…Ù†",
            1 => "Ø§Ù„Ø³Ø¨Ø§Ø¨Ø© Ø§Ù„ÙŠÙ…Ù†Ù‰",
            2 => "Ø§Ù„ÙˆØ³Ø·Ù‰ Ø§Ù„ÙŠÙ…Ù†Ù‰",
            3 => "Ø§Ù„Ø¨Ù†ØµØ± Ø§Ù„Ø£ÙŠÙ…Ù†",
            4 => "Ø§Ù„Ø®Ù†ØµØ± Ø§Ù„Ø£ÙŠÙ…Ù†",
            5 => "Ø§Ù„Ø¥Ø¨Ù‡Ø§Ù… Ø§Ù„Ø£ÙŠØ³Ø±",
            6 => "Ø§Ù„Ø³Ø¨Ø§Ø¨Ø© Ø§Ù„ÙŠØ³Ø±Ù‰",
            7 => "Ø§Ù„ÙˆØ³Ø·Ù‰ Ø§Ù„ÙŠØ³Ø±Ù‰",
            8 => "Ø§Ù„Ø¨Ù†ØµØ± Ø§Ù„Ø£ÙŠØ³Ø±",
            9 => "Ø§Ù„Ø®Ù†ØµØ± Ø§Ù„Ø£ÙŠØ³Ø±",
            _ => $"Ø¥ØµØ¨Ø¹ {index}"
        };
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR.Domain.Common;

namespace HR.Domain.Entities;

/// <summary>
/// يمثل بصمة إصبع واحدة لمستخدم معين
/// </summary>
public class Fingerprint : BaseEntity
{
    /// <summary>
    /// معرف المستخدم المرتبط بهذه البصمة (من الجهاز)
    /// </summary>
    [Required]
    public string DeviceUserId { get; set; } = string.Empty;

    /// <summary>
    /// اسم المستخدم المرتبط بهذه البصمة
    /// </summary>
    public string? Username { get; set; }
    public int UserId { get; set; }

    /// <summary>
    /// رقم الإصبع (0-9)
    /// 0: الإبهام الأيمن, 1: السبابة اليمنى, 2: الوسطى اليمنى, 3: البنصر الأيمن, 4: الخنصر الأيمن
    /// </summary>
    [Required]
    [Range(0, 9)]
    public int FingerIndex { get; set; }

    /// <summary>
    /// البصمة الفعلية (Template) كبيانات ثنائية
    /// </summary>
    [Required]
    public byte[] Template { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// حجم البصمة بالبايتات
    /// </summary>
    [Required]
    public int TemplateSize { get; set; }

    /// <summary>
    /// عنوان IP للجهاز الذي تم استخراج البصمة منه
    /// </summary>
    [MaxLength(50)]
    public string? DeviceIp { get; set; }

    /// <summary>
    /// هل البصمة صالحة للاستخدام؟
    /// </summary>
    public bool IsValid { get; set; } = true;


    /// <summary>
    /// المستخدم المرتبط بهذه البصمة
    /// </summary>
    [ForeignKey("UserId")]
    public virtual UserInfo? User { get; set; }

    /// <summary>
    /// الحصول على اسم الإصبع بالعربية
    /// </summary>
    [NotMapped]
    public string FingerName => GetFingerName(FingerIndex);

    /// <summary>
    /// تحويل رقم الإصبع إلى اسم بالعربية
    /// </summary>
    public static string GetFingerName(int index)
    {
        return index switch
        {
            0 => "الإبهام الأيمن",
            1 => "السبابة اليمنى",
            2 => "الوسطى اليمنى",
            3 => "البنصر الأيمن",
            4 => "الخنصر الأيمن",
            5 => "الإبهام الأيسر",
            6 => "السبابة اليسرى",
            7 => "الوسطى اليسرى",
            8 => "البنصر الأيسر",
            9 => "الخنصر الأيسر",
            _ => $"إصبع {index}"
        };
    }
}

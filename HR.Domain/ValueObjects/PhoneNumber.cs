// Domain/ValueObjects/PhoneNumber.cs
namespace HR.Domain.ValueObjects;
using HR.Domain.Common;
public record PhoneNumber
{
    private const int MIN_LENGTH = 10;
    private const int MAX_LENGTH = 11;

    // Ø£Ø±Ù‚Ø§Ù… Ø´Ø¨ÙƒØ§Øª Ø§Ù„Ø¹Ø±Ø§Ù‚
    private static readonly string[] ValidPrefixes = 
    {
        "0750", "0751", "0752", "0753", "0754", // Ø²ÙŠÙ†
        "0770", "0771", "0772", "0773", "0774", "0775", "0776", "0777", "0778", "0779", // Ø¢Ø³ÙŠØ§Ø³ÙŠÙ„
        "0780", "0781", "0782", "0783", "0784", "0785", "0786", "0787", "0788", "0789" // ÙƒÙˆØ±Ùƒ
    };

    public string Value { get; private set; }
    protected PhoneNumber() { Value = null!; } // ðŸ”¥ EF Core ONLY

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PhoneNumber>.Failure("Ø±Ù‚Ù… Ø§Ù„Ù‡Ø§ØªÙ Ù…Ø·Ù„ÙˆØ¨");

        // Ø¥Ø²Ø§Ù„Ø© Ø§Ù„Ù…Ø³Ø§ÙØ§Øª ÙˆØ§Ù„Ø±Ù…ÙˆØ²
        var cleaned = CleanPhoneNumber(value);

        // Ø§Ù„ØªØ­Ù‚Ù‚ Ù…Ù† Ø§Ù„Ø·ÙˆÙ„
        if (cleaned.Length < MIN_LENGTH || cleaned.Length > MAX_LENGTH)
            return Result<PhoneNumber>.Failure(
                $"Ø±Ù‚Ù… Ø§Ù„Ù‡Ø§ØªÙ ÙŠØ¬Ø¨ Ø£Ù† ÙŠÙƒÙˆÙ† Ø¨ÙŠÙ† {MIN_LENGTH} Ùˆ {MAX_LENGTH} Ø±Ù‚Ù…");

        // Ø§Ù„ØªØ­Ù‚Ù‚ Ù…Ù† Ø£Ù†Ù‡ Ø£Ø±Ù‚Ø§Ù… ÙÙ‚Ø·
        if (!cleaned.All(char.IsDigit))
            return Result<PhoneNumber>.Failure("Ø±Ù‚Ù… Ø§Ù„Ù‡Ø§ØªÙ ÙŠØ¬Ø¨ Ø£Ù† ÙŠØ­ØªÙˆÙŠ Ø¹Ù„Ù‰ Ø£Ø±Ù‚Ø§Ù… ÙÙ‚Ø·");

        // Ø¥Ø¶Ø§ÙØ© 0 ÙÙŠ Ø§Ù„Ø¨Ø¯Ø§ÙŠØ© Ø¥Ø°Ø§ Ù„Ù… ØªÙƒÙ† Ù…ÙˆØ¬ÙˆØ¯Ø©
        if (!cleaned.StartsWith("0"))
            cleaned = "0" + cleaned;

        // Ø§Ù„ØªØ­Ù‚Ù‚ Ù…Ù† Ø§Ù„Ø¨Ø§Ø¯Ø¦Ø© Ø§Ù„Ø¹Ø±Ø§Ù‚ÙŠØ©
        if (!IsValidIraqiPrefix(cleaned))
            return Result<PhoneNumber>.Failure(
                "Ø±Ù‚Ù… Ø§Ù„Ù‡Ø§ØªÙ Ù„Ø§ ÙŠØ·Ø§Ø¨Ù‚ Ø´Ø¨ÙƒØ§Øª Ø§Ù„Ø§ØªØµØ§Ù„ Ø§Ù„Ø¹Ø±Ø§Ù‚ÙŠØ© Ø§Ù„Ù…Ø¹Ø±ÙˆÙØ©");

        return Result<PhoneNumber>.Success(new PhoneNumber(cleaned));
    }

    private static string CleanPhoneNumber(string phone)
    {
        // Ø¥Ø²Ø§Ù„Ø© Ø§Ù„Ù…Ø³Ø§ÙØ§Øª ÙˆØ§Ù„Ø±Ù…ÙˆØ² Ø§Ù„Ø®Ø§ØµØ© ÙÙ‚Ø·
        return new string(phone.Where(c => char.IsDigit(c)).ToArray());
    }

    private static bool IsValidIraqiPrefix(string phone)
    {
        return ValidPrefixes.Any(prefix => phone.StartsWith(prefix));
    }

    // Ø§Ù„Ø­ØµÙˆÙ„ Ø¹Ù„Ù‰ Ø§Ù„Ø±Ù‚Ù… Ø¨ØµÙŠØºØ© Ù…Ù†Ø³Ù‚Ø©
    public string GetFormatted()
    {
        // 0750 123 4567
        if (Value.Length == 11)
            return $"{Value.Substring(0, 4)} {Value.Substring(4, 3)} {Value.Substring(7, 4)}";
        
        // 0750 12 3456
        return $"{Value.Substring(0, 4)} {Value.Substring(4, 2)} {Value.Substring(6, 4)}";
    }

    // Ù…Ø¹Ø±ÙØ© Ø§Ù„Ø´Ø¨ÙƒØ©
    public string GetNetworkProvider()
    {
        var prefix = Value.Substring(0, 4);
        return prefix switch
        {
            string p when p.StartsWith("075") => "Ø²ÙŠÙ†",
            string p when p.StartsWith("077") => "Ø¢Ø³ÙŠØ§Ø³ÙŠÙ„",
            string p when p.StartsWith("078") => "ÙƒÙˆØ±Ùƒ",
            _ => "ØºÙŠØ± Ù…Ø¹Ø±ÙˆÙ"
        };
    }

    // Ù„Ù„Ø¹Ø±Ø¶
    public override string ToString() => Value;

    // Ù„Ù„Ù…Ù‚Ø§Ø±Ù†Ø©
    public bool IsSameAs(PhoneNumber other) => Value == other.Value;
}

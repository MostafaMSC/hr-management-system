namespace HR.Domain.ValueObjects;
using HR.Domain.Common;
public record Money
{
    private const string CURRENCY = "IQD"; // Ø¹Ù…Ù„Ø© Ø«Ø§Ø¨ØªØ©
    private const decimal MIN_AMOUNT = 0;
    private const decimal MAX_AMOUNT = 999_999_999_999; // ØªØ±ÙŠÙ„ÙŠÙˆÙ† Ø¯ÙŠÙ†Ø§Ø±

    public decimal Amount { get; private set; }

    protected Money() { }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Result<Money> Create(decimal amount)
    {
        if (amount < MIN_AMOUNT)
            return Result<Money>.Failure("Ø§Ù„Ù…Ø¨Ù„Øº Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø£Ù† ÙŠÙƒÙˆÙ† Ø³Ø§Ù„Ø¨Ø§Ù‹");

        if (amount > MAX_AMOUNT)
            return Result<Money>.Failure("Ø§Ù„Ù…Ø¨Ù„Øº ÙŠØªØ¬Ø§ÙˆØ² Ø§Ù„Ø­Ø¯ Ø§Ù„Ø£Ù‚ØµÙ‰ Ø§Ù„Ù…Ø³Ù…ÙˆØ­");

        // ØªÙ‚Ø±ÙŠØ¨ Ù„Ø£Ù‚Ø±Ø¨ Ø¯ÙŠÙ†Ø§Ø± (Ù„Ø§ ØªÙˆØ¬Ø¯ ÙÙ„ÙˆØ³)
        var rounded = Math.Round(amount, 0);

        return Result<Money>.Success(new Money(rounded));
    }

    public static Money Zero => new(0);

    // Ø§Ù„Ø¹Ù…Ù„ÙŠØ§Øª Ø§Ù„Ø­Ø³Ø§Ø¨ÙŠØ©
    public Money Add(Money other)
    {
        var result = Create(Amount + other.Amount);
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error);
        return result.Value;
    }

    public Money Subtract(Money other)
    {
        var result = Create(Amount - other.Amount);
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error);
        return result.Value;
    }

    public Money Multiply(decimal multiplier)
    {
        var result = Create(Amount * multiplier);
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error);
        return result.Value;
    }

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø§Ù„Ù‚Ø³Ù…Ø© Ø¹Ù„Ù‰ ØµÙØ±");

        var result = Create(Amount / divisor);
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error);
        return result.Value;
    }

    // Ø§Ù„Ù…Ù‚Ø§Ø±Ù†Ø§Øª
    public bool IsGreaterThan(Money other) => Amount > other.Amount;
    public bool IsLessThan(Money other) => Amount < other.Amount;
    public bool IsGreaterThanOrEqual(Money other) => Amount >= other.Amount;
    public bool IsLessThanOrEqual(Money other) => Amount <= other.Amount;
    public bool IsZero() => Amount == 0;

    // Ø§Ù„ØªÙ†Ø³ÙŠÙ‚
    public string Format() => $"{Amount:N0} {CURRENCY}";
    public string FormatShort() => $"{Amount:N0}"; // Ø¨Ø¯ÙˆÙ† Ø¹Ù…Ù„Ø©
    
    public override string ToString() => Format();

    // Operators Ù„Ù„ØªØ³Ù‡ÙŠÙ„
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal multiplier) => money.Multiply(multiplier);
    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);
    
    public static bool operator >(Money left, Money right) => left.IsGreaterThan(right);
    public static bool operator <(Money left, Money right) => left.IsLessThan(right);
    public static bool operator >=(Money left, Money right) => left.IsGreaterThanOrEqual(right);
    public static bool operator <=(Money left, Money right) => left.IsLessThanOrEqual(right);
}

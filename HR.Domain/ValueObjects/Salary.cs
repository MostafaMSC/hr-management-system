namespace HR.Domain.ValueObjects;
using HR.Domain.Common;    
public record Salary
{
    private const decimal MINIMUM_WAGE = 500_000;
    private const decimal MAX_HOUSING_PERCENTAGE = 0.5m;
    private const decimal MAX_TRANSPORT_AMOUNT = 200_000;
    private const decimal MAX_DEDUCTION_PERCENTAGE = 0.3m;

    public Money BaseSalary { get; private set; }
    public Money? HousingAllowance { get; private set; }
    public Money? TransportAllowance { get; private set; }
    public Money? OtherAllowances { get; private set; }

    protected Salary() { BaseSalary = null!; }

    private Salary(
        Money baseSalary,
        Money? housingAllowance,
        Money? transportAllowance,
        Money? otherAllowances)
    {
        BaseSalary = baseSalary;
        HousingAllowance = housingAllowance;
        TransportAllowance = transportAllowance;
        OtherAllowances = otherAllowances;
    }

    public static Result<Salary> Create(
        Money baseSalary,
        Money? housingAllowance = null,
        Money? transportAllowance = null,
        Money? otherAllowances = null)
    {
        if (baseSalary.Amount < MINIMUM_WAGE)
            return Result<Salary>.Failure(
                $"Ø§Ù„Ø±Ø§ØªØ¨ Ø§Ù„Ø£Ø³Ø§Ø³ÙŠ Ø£Ù‚Ù„ Ù…Ù† Ø§Ù„Ø­Ø¯ Ø§Ù„Ø£Ø¯Ù†Ù‰ ({MINIMUM_WAGE:N0} IQD)");
        if (housingAllowance != null)
        {
            var maxHousing = baseSalary.Amount * MAX_HOUSING_PERCENTAGE;
            if (housingAllowance.Amount > maxHousing)
                return Result<Salary>.Failure(
                    $"Ø¹Ù„Ø§ÙˆØ© Ø§Ù„Ø³ÙƒÙ† ØªØªØ¬Ø§ÙˆØ² {MAX_HOUSING_PERCENTAGE:P0} Ù…Ù† Ø§Ù„Ø±Ø§ØªØ¨ Ø§Ù„Ø£Ø³Ø§Ø³ÙŠ");
        }

        if (transportAllowance != null)
        {
            if (transportAllowance.Amount > MAX_TRANSPORT_AMOUNT)
                return Result<Salary>.Failure(
                    $"Ø¹Ù„Ø§ÙˆØ© Ø§Ù„Ù†Ù‚Ù„ ØªØªØ¬Ø§ÙˆØ² Ø§Ù„Ø­Ø¯ Ø§Ù„Ø£Ù‚ØµÙ‰ ({MAX_TRANSPORT_AMOUNT:N0} IQD)");
        }

        return Result<Salary>.Success(new Salary(
            baseSalary,
            housingAllowance,
            transportAllowance,
            otherAllowances));
    }

    public Money GetGrossSalary()
    {
        var total = BaseSalary;

        if (HousingAllowance != null)
            total = total.Add(HousingAllowance);

        if (TransportAllowance != null)
            total = total.Add(TransportAllowance);

        if (OtherAllowances != null)
            total = total.Add(OtherAllowances);

        return total;
    }

         // Ø­Ø³Ø§Ø¨ Ø§Ù„Ù…Ø¹Ø¯Ù„ Ø§Ù„ÙŠÙˆÙ…ÙŠ
    public Money GetDailyRate(int daysInMonth = 30)
    {
        return BaseSalary.Divide(daysInMonth);
    }

    // Ø­Ø³Ø§Ø¨ Ø§Ù„Ù…Ø¹Ø¯Ù„ Ø¨Ø§Ù„Ø³Ø§Ø¹Ø©      
    public Money GetHourlyRate(int workingHoursPerDay = 8, int daysInMonth = 30)
    {
        var totalHours = workingHoursPerDay * daysInMonth;
        return BaseSalary.Divide(totalHours);
    }

    // Ø­Ø³Ø§Ø¨ Ø§Ù„Ø®ØµÙ… Ø¨Ù†Ø§Ø¡Ù‹ Ø¹Ù„Ù‰ Ø£ÙŠØ§Ù… Ø§Ù„ØªØ£Ø®ÙŠØ±
    public Money CalculateDeductionForLateDays(int lateDays, int daysInMonth = 30)
    {
        if (lateDays <= 0)
            return Money.Zero;

        var dailyRate = GetDailyRate(daysInMonth);
        var deduction = dailyRate.Multiply(lateDays);

        // Ø§Ù„ØªØ£ÙƒØ¯ Ù…Ù† Ø¹Ø¯Ù… ØªØ¬Ø§ÙˆØ² Ø§Ù„Ø­Ø¯ Ø§Ù„Ø£Ù‚ØµÙ‰ Ù„Ù„Ø®ØµÙ…
        var maxDeduction = GetMaximumDeduction();
        return deduction.IsGreaterThan(maxDeduction) ? maxDeduction : deduction;
    }

    // Ø­Ø³Ø§Ø¨ Ø§Ù„Ø®ØµÙ… Ø¨Ù†Ø§Ø¡Ù‹ Ø¹Ù„Ù‰ Ø³Ø§Ø¹Ø§Øª Ø§Ù„ØºÙŠØ§Ø¨
    public Money CalculateDeductionForAbsentHours(
        int absentHours,
        int workingHoursPerDay = 8,
        int daysInMonth = 30)
    {
        if (absentHours <= 0)
            return Money.Zero;

        var hourlyRate = GetHourlyRate(workingHoursPerDay, daysInMonth);
        var deduction = hourlyRate.Multiply(absentHours);

        var maxDeduction = GetMaximumDeduction();
        return deduction.IsGreaterThan(maxDeduction) ? maxDeduction : deduction;
    }

    // Ø§Ù„Ø­Ø¯ Ø§Ù„Ø£Ù‚ØµÙ‰ Ù„Ù„Ø®ØµÙ… (30% Ù…Ù† Ø§Ù„Ø±Ø§ØªØ¨ Ø§Ù„Ø£Ø³Ø§Ø³ÙŠ Ø­Ø³Ø¨ Ø§Ù„Ù‚Ø§Ù†ÙˆÙ† Ø§Ù„Ø¹Ø±Ø§Ù‚ÙŠ)
    public Money GetMaximumDeduction()
    {
        return BaseSalary.Multiply(MAX_DEDUCTION_PERCENTAGE);
    }

    // Ø­Ø³Ø§Ø¨ Ø§Ù„Ø±Ø§ØªØ¨ Ø§Ù„ØµØ§ÙÙŠ Ø¨Ø¹Ø¯ Ø§Ù„Ø®ØµÙˆÙ…Ø§Øª
    public Money GetNetSalary(Money totalDeductions)
    {
        var gross = GetGrossSalary();
        return gross.Subtract(totalDeductions);
    }

    // Ø¥Ø¶Ø§ÙØ© Ø¹Ù„Ø§ÙˆØ©
    public Result<Salary> AddAllowance(Money allowance, string type)
    {
        return type.ToLower() switch
        {
            "housing" => Create(BaseSalary, allowance, TransportAllowance, OtherAllowances),
            "transport" => Create(BaseSalary, HousingAllowance, allowance, OtherAllowances),
            "other" => Create(BaseSalary, HousingAllowance, TransportAllowance, 
                OtherAllowances?.Add(allowance) ?? allowance),
            _ => Result<Salary>.Failure($"Ù†ÙˆØ¹ Ø§Ù„Ø¹Ù„Ø§ÙˆØ© ØºÙŠØ± Ù…Ø¹Ø±ÙˆÙ: {type}")
        };
    }

    // ØªØ­Ø¯ÙŠØ« Ø§Ù„Ø±Ø§ØªØ¨ Ø§Ù„Ø£Ø³Ø§Ø³ÙŠ
    public Result<Salary> UpdateBaseSalary(Money newBaseSalary)
    {
        return Create(newBaseSalary, HousingAllowance, TransportAllowance, OtherAllowances);
    }

    public override string ToString() => GetGrossSalary().Format();
}

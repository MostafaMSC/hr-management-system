using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Settings.Queries;

public record GetWorkSettingsQuery : IRequest<Dictionary<string, object>>;

public class GetWorkSettingsQueryHandler : IRequestHandler<GetWorkSettingsQuery, Dictionary<string, object>>
{
    private readonly ISettingsRepository _settingsRepository;

    public GetWorkSettingsQueryHandler(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Dictionary<string, object>> Handle(GetWorkSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetSettingsBySectionAsync("Work", cancellationToken);

        var result = new Dictionary<string, object>();
        foreach (var s in settings)
        {
            if (double.TryParse(s.Value, out double doubleVal))
                result[s.Key] = doubleVal;
            else if (bool.TryParse(s.Value, out bool boolVal))
                result[s.Key] = boolVal;
            else
                result[s.Key] = s.Value;
        }

        // Defaults if missing
        if (!result.ContainsKey("workDayStart")) result["workDayStart"] = "08:30";
        if (!result.ContainsKey("workDayEnd")) result["workDayEnd"] = "16:00";
        if (!result.ContainsKey("requiredDailyHours")) result["requiredDailyHours"] = 8;
        if (!result.ContainsKey("workingDaysPerMonth")) result["workingDaysPerMonth"] = 26;
        if (!result.ContainsKey("allowedMonthlyLeaveDays")) result["allowedMonthlyLeaveDays"] = 1.7;
        if (!result.ContainsKey("allowedMonthlyLeaveHours")) result["allowedMonthlyLeaveHours"] = 4;
        if (!result.ContainsKey("allowedSickLeaveDays")) result["allowedSickLeaveDays"] = 15;
        if (!result.ContainsKey("firstDayOfWeek")) result["firstDayOfWeek"] = 6;
        if (!result.ContainsKey("weekendDays")) result["weekendDays"] = "5,6";
        if (!result.ContainsKey("enableBirthdayGreetings")) result["enableBirthdayGreetings"] = true;

        // Dynamic Leave Limits
        if (!result.ContainsKey("maternityLeaveDays")) result["maternityLeaveDays"] = 98;
        if (!result.ContainsKey("paternityLeaveDays")) result["paternityLeaveDays"] = 2;
        if (!result.ContainsKey("hajjLeaveDays")) result["hajjLeaveDays"] = 30;
        if (!result.ContainsKey("marriageLeaveDays")) result["marriageLeaveDays"] = 5;
        if (!result.ContainsKey("bereavementLeaveDays")) result["bereavementLeaveDays"] = 3;
        if (!result.ContainsKey("unpaidLeaveDays")) result["unpaidLeaveDays"] = 30;

        return result;
    }
}

using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Reports.Queries;

public class DailyLateDto
{
    public DateTime Date { get; set; }
    public int DelayMinutes { get; set; }
}

public class WeeklyLateReportDto
{
    public string UserName { get; set; } = string.Empty;
    public int LateDaysCount { get; set; }
    public int TotalLateMinutes { get; set; }
    public List<DailyLateDto> LateDetails { get; set; } = new();
}

public class GetWeeklyLateQuery : IRequest<List<WeeklyLateReportDto>>
{
    // Optionally specify the week start date. Defaults to current week.
    public DateTime? WeekStartDate { get; set; }
}

public class GetWeeklyLateQueryHandler : IRequestHandler<GetWeeklyLateQuery, List<WeeklyLateReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWeeklyLateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WeeklyLateReportDto>> Handle(GetWeeklyLateQuery request, CancellationToken cancellationToken)
    {
        // Default to the most recent Saturday
        var startOfWeek = request.WeekStartDate?.Date ?? GetPreviousSaturday(DateTime.Today);
        var endOfWeek = startOfWeek.AddDays(6);

        var summaries = await _context.DailyAttendanceSummaries
            .Include(s => s.UserInfo)
            .Where(s => s.Date >= startOfWeek && s.Date <= endOfWeek && s.DelayMinutes > 0)
            .ToListAsync(cancellationToken);

        var report = summaries
            .GroupBy(s => s.UserInfo.FirstName + " " + s.UserInfo.LastName)
            .Select(g => new WeeklyLateReportDto
            {
                UserName = g.Key,
                LateDaysCount = g.Count(),
                TotalLateMinutes = g.Sum(s => s.DelayMinutes),
                LateDetails = g.Select(s => new DailyLateDto
                {
                    Date = s.Date,
                    DelayMinutes = s.DelayMinutes
                }).OrderBy(d => d.Date).ToList()
            })
            .OrderByDescending(r => r.LateDaysCount)
            .ThenByDescending(r => r.TotalLateMinutes)
            .ToList();

        return report;
    }

    private DateTime GetPreviousSaturday(DateTime date)
    {
        int offset = date.DayOfWeek == DayOfWeek.Saturday ? 0 : (int)DayOfWeek.Saturday - (int)date.DayOfWeek - 7;
        return date.AddDays(offset);
    }
}

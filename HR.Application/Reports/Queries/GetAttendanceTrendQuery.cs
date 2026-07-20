using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Reports.Queries;

public class AttendanceTrendReportDto
{
    public DateTime Date { get; set; }
    public int TotalRegular { get; set; }
    public int TotalAbsent { get; set; }
    public int TotalLate { get; set; }
    public int TotalOnLeave { get; set; }
}

public class GetAttendanceTrendQuery : IRequest<List<AttendanceTrendReportDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetAttendanceTrendQueryHandler : IRequestHandler<GetAttendanceTrendQuery, List<AttendanceTrendReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceTrendQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AttendanceTrendReportDto>> Handle(GetAttendanceTrendQuery request, CancellationToken cancellationToken)
    {
        var summaries = await _context.DailyAttendanceSummaries
            .Where(s => s.Date >= request.StartDate.Date && s.Date <= request.EndDate.Date)
            .GroupBy(s => s.Date)
            .Select(g => new AttendanceTrendReportDto
            {
                Date = g.Key,
                TotalRegular = g.Count(s => s.Status == AttendanceStatus.Regular && s.DelayMinutes == 0),
                TotalLate = g.Count(s => s.DelayMinutes > 0),
                TotalAbsent = g.Count(s => s.Status == AttendanceStatus.Absent),
                TotalOnLeave = g.Count(s => s.Status == AttendanceStatus.OnLeave)
            })
            .OrderBy(r => r.Date)
            .ToListAsync(cancellationToken);

        return summaries;
    }
}

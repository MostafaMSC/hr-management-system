using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Reports.Queries;

public class DailyLateReportDto
{
    public string UserName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? TimeIn { get; set; }
    public int DelayMinutes { get; set; }
}

public class GetLateQuery : IRequest<List<DailyLateReportDto>>
{
    public DateTime Date { get; set; }
}

public class GetLateQueryHandler : IRequestHandler<GetLateQuery, List<DailyLateReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyLateReportDto>> Handle(GetLateQuery request, CancellationToken cancellationToken)
    {
        var summaries = await _context.DailyAttendanceSummaries
            .Include(s => s.UserInfo)
            .ThenInclude(u => u.Department)
            .Where(s => s.Date == request.Date.Date && s.DelayMinutes > 0)
            .OrderByDescending(s => s.DelayMinutes)
            .Select(s => new DailyLateReportDto
            {
                UserName = s.UserInfo.FirstName + " " + s.UserInfo.LastName,
                DepartmentName = s.UserInfo.Department != null ? s.UserInfo.Department.Name : "N/A",
                TimeIn = s.TimeIn.HasValue ? s.TimeIn.Value.ToString(@"hh\:mm") : null,
                DelayMinutes = s.DelayMinutes
            })
            .ToListAsync(cancellationToken);

        return summaries;
    }
}

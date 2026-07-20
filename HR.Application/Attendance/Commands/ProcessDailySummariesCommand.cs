using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.Commands;

public class ProcessDailySummariesCommand : IRequest<bool>
{
    public int UserInfoId { get; set; }
    public DateTime TargetDate { get; set; }
}

public class ProcessDailySummariesCommandHandler : IRequestHandler<ProcessDailySummariesCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ProcessDailySummariesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ProcessDailySummariesCommand request, CancellationToken cancellationToken)
    {
        return true;
    }
}

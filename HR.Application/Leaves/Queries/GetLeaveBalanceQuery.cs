using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Leaves.Queries;

public class LeaveBalanceDto
{
    public int Year { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public int TotalAllowed { get; set; }
    public int Used { get; set; }
    public int Remaining { get; set; }
}

public record GetLeaveBalanceQuery(int UserId) : IRequest<List<LeaveBalanceDto>>;

public class GetLeaveBalanceQueryHandler : IRequestHandler<GetLeaveBalanceQuery, List<LeaveBalanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveBalanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveBalanceDto>> Handle(GetLeaveBalanceQuery request, CancellationToken cancellationToken)
    {
        var currentYear = DateTime.UtcNow.Year;

        var balances = await _context.LeaveBalances
            .Where(b => b.UserInfoId == request.UserId && b.Year == currentYear)
            .ToListAsync(cancellationToken);

        return balances.Select(b => new LeaveBalanceDto
        {
            Year = b.Year,
            LeaveType = b.LeaveType,
            TotalAllowed = b.TotalAllowed,
            Used = b.Used,
            Remaining = b.Remaining
        }).ToList();
    }
}

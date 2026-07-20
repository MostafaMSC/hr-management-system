using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Bonuses.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Bonuses.Queries;

public record GetBonusRequestsQuery(int? ManagerId, int? TargetUserId, BonusStatus? Status, int? Year, int? Month) : IRequest<List<BonusRequestDto>>;

public class GetBonusRequestsQueryHandler : IRequestHandler<GetBonusRequestsQuery, List<BonusRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBonusRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BonusRequestDto>> Handle(GetBonusRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BonusRequests
            .Include(b => b.RequestingManager)
            .Include(b => b.TargetUser)
            .Include(b => b.ProcessedByHr)
            .AsQueryable();

        if (request.ManagerId.HasValue)
            query = query.Where(b => b.RequestingManagerId == request.ManagerId.Value);

        if (request.TargetUserId.HasValue)
            query = query.Where(b => b.TargetUserId == request.TargetUserId.Value);

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        if (request.Year.HasValue)
            query = query.Where(b => b.Year == request.Year.Value);

        if (request.Month.HasValue)
            query = query.Where(b => b.Month == request.Month.Value);

        var results = await query.OrderByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);

        return results.Select(b => new BonusRequestDto
        {
            Id = b.Id,
            RequestingManagerId = b.RequestingManagerId,
            RequestingManagerName = b.RequestingManager?.Username ?? "",
            TargetUserId = b.TargetUserId,
            TargetUserName = b.TargetUser?.Username ?? "",
            Type = b.Type,
            Value = b.Value,
            Reason = b.Reason,
            Status = b.Status,
            ProcessedByHrId = b.ProcessedByHrId,
            ProcessedByHrName = b.ProcessedByHr?.Username,
            ProcessedAt = b.ProcessedAt,
            Year = b.Year,
            Month = b.Month,
            CreatedAt = b.CreatedAt
        }).ToList();
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Bonuses.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

using HR.Application.Common.Models;

namespace HR.Application.Bonuses.Queries;

public record GetBonusRequestsQuery(int? ManagerId, int? TargetUserId, BonusStatus? Status, int? Year, int? Month, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<BonusRequestDto>>;

public class GetBonusRequestsQueryHandler : IRequestHandler<GetBonusRequestsQuery, PaginatedResult<BonusRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBonusRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<BonusRequestDto>> Handle(GetBonusRequestsQuery request, CancellationToken cancellationToken)
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

        var totalCount = await query.CountAsync(cancellationToken);

        var results = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var data = results.Select(b => new BonusRequestDto
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

        return new PaginatedResult<BonusRequestDto>(data, totalCount, request.PageNumber, request.PageSize);
    }
}

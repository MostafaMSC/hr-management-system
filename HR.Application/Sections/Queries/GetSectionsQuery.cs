using HR.Application.Common.Interfaces;
using HR.Application.Sections.DTOs;
using MediatR;

using HR.Application.Common.Models;

namespace HR.Application.Sections.Queries;

public record GetSectionsQuery(int? DepartmentId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<SectionDto>>;

public class GetSectionsQueryHandler : IRequestHandler<GetSectionsQuery, PaginatedResult<SectionDto>>
{
    private readonly ISectionRepository _sectionRepository;

    public GetSectionsQueryHandler(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<PaginatedResult<SectionDto>> Handle(GetSectionsQuery request, CancellationToken cancellationToken)
    {
        var sections = await _sectionRepository.GetAllAsync(request.DepartmentId);
        
        var totalCount = sections.Count();
        
        var data = sections
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SectionDto
        {
            Id = s.Id,
            Name = s.Name,
            DepartmentId = s.DepartmentId,
            DepartmentName = s.Department?.Name ?? string.Empty,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();

        return new PaginatedResult<SectionDto>(data, totalCount, request.PageNumber, request.PageSize);
    }
}

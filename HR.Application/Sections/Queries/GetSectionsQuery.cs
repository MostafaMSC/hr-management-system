using HR.Application.Common.Interfaces;
using HR.Application.Sections.DTOs;
using MediatR;

namespace HR.Application.Sections.Queries;

public record GetSectionsQuery(int? DepartmentId = null) : IRequest<List<SectionDto>>;

public class GetSectionsQueryHandler : IRequestHandler<GetSectionsQuery, List<SectionDto>>
{
    private readonly ISectionRepository _sectionRepository;

    public GetSectionsQueryHandler(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<List<SectionDto>> Handle(GetSectionsQuery request, CancellationToken cancellationToken)
    {
        var sections = await _sectionRepository.GetAllAsync(request.DepartmentId);

        return sections.Select(s => new SectionDto
        {
            Id = s.Id,
            Name = s.Name,
            DepartmentId = s.DepartmentId,
            DepartmentName = s.Department?.Name ?? string.Empty,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }
}

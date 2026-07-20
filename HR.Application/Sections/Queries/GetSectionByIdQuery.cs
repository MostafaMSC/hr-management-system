using HR.Application.Common.Interfaces;
using HR.Application.Sections.DTOs;
using MediatR;

namespace HR.Application.Sections.Queries;

public record GetSectionByIdQuery(int Id) : IRequest<SectionDto?>;

public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, SectionDto?>
{
    private readonly ISectionRepository _sectionRepository;

    public GetSectionByIdQueryHandler(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<SectionDto?> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
    {
        var section = await _sectionRepository.GetByIdAsync(request.Id);

        if (section == null) return null;

        return new SectionDto
        {
            Id = section.Id,
            Name = section.Name,
            DepartmentId = section.DepartmentId,
            DepartmentName = section.Department?.Name ?? string.Empty,
            Description = section.Description,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt
        };
    }
}

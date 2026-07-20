using HR.Application.Common.Interfaces;
using HR.Application.Sections.DTOs;
using MediatR;

namespace HR.Application.Sections.Commands;

public record UpdateSectionCommand(int Id, string Name, int DepartmentId, string? Description = null) : IRequest<SectionDto>;

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, SectionDto>
{
    private readonly ISectionRepository _sectionRepository;

    public UpdateSectionCommandHandler(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<SectionDto> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _sectionRepository.GetByIdAsync(request.Id);
        if (section == null)
        {
            throw new KeyNotFoundException($"Section with ID {request.Id} not found.");
        }

        // Check name uniqueness if changed
        if (!string.Equals(section.Name, request.Name, StringComparison.OrdinalIgnoreCase) || section.DepartmentId != request.DepartmentId)
        {
            var existing = await _sectionRepository.GetByNameAsync(request.Name, request.DepartmentId);
            if (existing != null && existing.Id != request.Id)
            {
                throw new InvalidOperationException($"A section with the name '{request.Name}' already exists in this department.");
            }
        }

        section.Name = request.Name;
        section.DepartmentId = request.DepartmentId;
        section.Description = request.Description;

        var updatedSection = await _sectionRepository.UpdateAsync(section);

        // Reload to get department name
        updatedSection = await _sectionRepository.GetByIdAsync(updatedSection.Id);

        return new SectionDto
        {
            Id = updatedSection!.Id,
            Name = updatedSection.Name,
            DepartmentId = updatedSection.DepartmentId,
            DepartmentName = updatedSection.Department?.Name ?? string.Empty,
            Description = updatedSection.Description,
            CreatedAt = updatedSection.CreatedAt,
            UpdatedAt = updatedSection.UpdatedAt
        };
    }
}

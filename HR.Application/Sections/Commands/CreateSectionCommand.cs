using HR.Application.Common.Interfaces;
using HR.Application.Sections.DTOs;
using HR.Domain.Entities;
using MediatR;

namespace HR.Application.Sections.Commands;

public record CreateSectionCommand(string Name, int DepartmentId, string? Description = null) : IRequest<SectionDto>;

public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, SectionDto>
{
    private readonly ISectionRepository _sectionRepository;

    public CreateSectionCommandHandler(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<SectionDto> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        // Check if section with same name exists in department
        var existing = await _sectionRepository.GetByNameAsync(request.Name, request.DepartmentId);
        if (existing != null)
        {
            throw new InvalidOperationException($"A section with the name '{request.Name}' already exists in this department.");
        }

        var section = new Section
        {
            Name = request.Name,
            DepartmentId = request.DepartmentId,
            Description = request.Description
        };

        var createdSection = await _sectionRepository.AddAsync(section);

        // Reload to get department name
        createdSection = await _sectionRepository.GetByIdAsync(createdSection.Id);

        return new SectionDto
        {
            Id = createdSection!.Id,
            Name = createdSection.Name,
            DepartmentId = createdSection.DepartmentId,
            DepartmentName = createdSection.Department?.Name ?? string.Empty,
            Description = createdSection.Description,
            CreatedAt = createdSection.CreatedAt,
            UpdatedAt = createdSection.UpdatedAt
        };
    }
}

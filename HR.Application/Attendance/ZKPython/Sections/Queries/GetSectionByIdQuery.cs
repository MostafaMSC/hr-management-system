using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Sections.Queries;

public record GetSectionByIdQuery(int Id) : IRequest<SectionDto?>;

public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, SectionDto?>
{
    private readonly ISectionRepository _repository;

    public GetSectionByIdQueryHandler(ISectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<SectionDto?> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
    {
        var section = await _repository.GetByIdAsync(request.Id);
        if (section == null) return null;

        return new SectionDto
        {
            Id = section.Id,
            Name = section.Name,
            DepartmentId = section.DepartmentId,
            DepartmentName = section.Department?.Name ?? "Unknown",
            Description = section.Description,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt ?? DateTime.UtcNow
        };
    }
}

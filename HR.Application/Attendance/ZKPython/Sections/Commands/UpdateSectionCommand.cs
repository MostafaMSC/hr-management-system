using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using HR.Application.Common;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Sections.Commands;

public record UpdateSectionCommand(int Id, string Name, int DepartmentId, string? Description = null) : IRequest<SectionDto>;

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, SectionDto>
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICacheService _cache;

    public UpdateSectionCommandHandler(ISectionRepository sectionRepository, IDepartmentRepository departmentRepository, ICacheService cache)
    {
        _sectionRepository = sectionRepository;
        _departmentRepository = departmentRepository;
        _cache = cache;
    }

    public async Task<SectionDto> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        return null!;
    }
}

using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using HR.Application.Common;
using HR.Domain.Entities;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Departments.Commands;

public record CreateDepartmentCommand(string Name, string? Description = null) : IRequest<DepartmentDto>;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _repository;
    private readonly ICacheService _cache;

    public CreateDepartmentCommandHandler(IDepartmentRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        return null!;
    }
}

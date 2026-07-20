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

namespace HR.Application.Attendance.ZKPython.Departments.Commands;

public record UpdateDepartmentCommand(int Id, string Name, string? Description = null) : IRequest<DepartmentDto>;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    public UpdateDepartmentCommandHandler() { }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        return null!;
    }
}

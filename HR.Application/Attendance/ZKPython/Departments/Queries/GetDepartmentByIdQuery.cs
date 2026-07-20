using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Departments.Queries;

public record GetDepartmentByIdQuery(int Id) : IRequest<DepartmentDto?>;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    private readonly IDepartmentRepository _repository;

    public GetDepartmentByIdQueryHandler(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        return null;
    }
}

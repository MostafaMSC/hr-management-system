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
using System.Collections.Generic;

namespace HR.Application.Attendance.ZKPython.Departments.Queries;

public record GetDepartmentsQuery() : IRequest<List<DepartmentDto>>;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, List<DepartmentDto>>
{
    public GetDepartmentsQueryHandler() { }

    public async Task<List<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        return new List<DepartmentDto>();
    }
}

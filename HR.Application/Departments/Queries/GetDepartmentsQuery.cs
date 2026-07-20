using HR.Application.Common.Interfaces;
using HR.Application.Departments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Departments.Queries;

public class GetDepartmentsQuery : IRequest<List<DepartmentDto>>
{
}

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, List<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                ManagerId = d.ManagerId,
                ManagerName = d.Manager != null ? d.Manager.FirstName + " " + d.Manager.LastName : null
            })
            .ToListAsync(cancellationToken);
    }
}

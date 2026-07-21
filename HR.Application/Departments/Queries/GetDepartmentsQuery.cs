using HR.Application.Common.Interfaces;
using HR.Application.Departments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

using HR.Application.Common.Models;

namespace HR.Application.Departments.Queries;

public record GetDepartmentsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<DepartmentDto>>;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, PaginatedResult<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _context.Departments.CountAsync(cancellationToken);

        var data = await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                ManagerId = d.ManagerId,
                ManagerName = d.Manager != null ? d.Manager.FirstName + " " + d.Manager.LastName : null
            })
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<DepartmentDto>(data, totalCount, request.PageNumber, request.PageSize);
    }
}

using HR.Application.Common.Interfaces;
using HR.Application.Departments.DTOs;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Departments.Queries;

public record GetDepartmentByIdQuery(int Id) : IRequest<DepartmentDto>;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                ManagerId = d.ManagerId,
                ManagerName = d.Manager != null ? d.Manager.FirstName + " " + d.Manager.LastName : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (department == null)
            throw new NotFoundException($"Department with ID {request.Id} not found.");

        return department;
    }
}

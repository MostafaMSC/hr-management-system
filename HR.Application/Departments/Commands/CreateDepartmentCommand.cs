using HR.Application.Common.Interfaces;
using HR.Application.Departments.DTOs;
using HR.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Departments.Commands;

public class CreateDepartmentCommand : IRequest<DepartmentDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (request.ManagerId.HasValue)
        {
            var managerExists = await _context.UserInfos.AnyAsync(u => u.Id == request.ManagerId.Value && !u.IsDeleted, cancellationToken);
            if (!managerExists)
                throw new InvalidOperationException($"Manager with ID {request.ManagerId.Value} not found.");
        }

        var entity = new Department
        {
            Name = request.Name,
            Description = request.Description,
            ManagerId = request.ManagerId
        };

        _context.Departments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        string? managerName = null;
        if (entity.ManagerId.HasValue)
        {
            managerName = await _context.UserInfos
                .Where(u => u.Id == entity.ManagerId.Value)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new DepartmentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ManagerId = entity.ManagerId,
            ManagerName = managerName
        };
    }
}

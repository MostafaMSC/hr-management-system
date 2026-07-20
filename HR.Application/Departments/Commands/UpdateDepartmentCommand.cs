using HR.Application.Common.Interfaces;
using HR.Application.Departments.DTOs;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Departments.Commands;

public class UpdateDepartmentCommand : IRequest<DepartmentDto>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Departments
            .Include(d => d.Manager)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException($"Department with ID {request.Id} not found.");

        if (request.ManagerId.HasValue && request.ManagerId != entity.ManagerId)
        {
            var managerExists = await _context.UserInfos.AnyAsync(u => u.Id == request.ManagerId.Value && !u.IsDeleted, cancellationToken);
            if (!managerExists)
                throw new InvalidOperationException($"Manager with ID {request.ManagerId.Value} not found.");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.ManagerId = request.ManagerId;

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

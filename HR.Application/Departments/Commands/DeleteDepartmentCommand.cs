using HR.Application.Common.Interfaces;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Departments.Commands;

public record DeleteDepartmentCommand(int Id) : IRequest<bool>;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException($"Department with ID {request.Id} not found.");

        if (entity.Employees.Any())
            throw new InvalidOperationException("Cannot delete department because it has employees assigned to it.");

        _context.Departments.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

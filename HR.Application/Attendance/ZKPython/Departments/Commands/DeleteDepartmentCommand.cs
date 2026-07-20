using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using HR.Application.Common;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Departments.Commands;

public record DeleteDepartmentCommand(int Id) : IRequest<bool>;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
{
    private readonly IDepartmentRepository _repository;
    private readonly ICacheService _cache;

    public DeleteDepartmentCommandHandler(IDepartmentRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        return true;
    }
}

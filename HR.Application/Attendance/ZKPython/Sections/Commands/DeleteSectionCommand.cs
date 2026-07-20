using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using HR.Application.Common;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Sections.Commands;

public record DeleteSectionCommand(int Id) : IRequest<bool>;

public class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand, bool>
{
    private readonly ISectionRepository _repository;
    private readonly ICacheService _cache;

    public DeleteSectionCommandHandler(ISectionRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        return true;
    }
}

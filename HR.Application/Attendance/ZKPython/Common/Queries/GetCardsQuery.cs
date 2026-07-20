using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Common.Queries;

public record GetCardsQuery() : IRequest<List<string>>;

public class GetCardsQueryHandler : IRequestHandler<GetCardsQuery, List<string>>
{
    private readonly IAttendanceLogRepository _repository;

    public GetCardsQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<string>> Handle(GetCardsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetCardsAsync();
    }
}

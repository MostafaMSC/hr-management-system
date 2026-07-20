using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.UserDevices.Queries;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.UserDevices.Queries;

public class GetUserHRsQueryHandler : IRequestHandler<GetUserHRsQuery, List<HRDto>>
{
    private readonly IFingerprintRepository _HRRepository;
    private readonly ILogger<GetUserHRsQueryHandler> _logger;

    public GetUserHRsQueryHandler(
        IFingerprintRepository HRRepository,
        ILogger<GetUserHRsQueryHandler> logger)
    {
        _HRRepository = HRRepository;
        _logger = logger;
    }

    public async Task<List<HRDto>> Handle(GetUserHRsQuery request, CancellationToken cancellationToken)
    {
        return new List<HRDto>();
    }
}

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

public class GetAllHRsQueryHandler : IRequestHandler<GetAllHRsQuery, List<HRDto>>
{
    private readonly IFingerprintRepository _HRRepository;
    private readonly ILogger<GetAllHRsQueryHandler> _logger;

    public GetAllHRsQueryHandler(
        IFingerprintRepository HRRepository,
        ILogger<GetAllHRsQueryHandler> logger)
    {
        _HRRepository = HRRepository;
        _logger = logger;
    }

    public async Task<List<HRDto>> Handle(GetAllHRsQuery request, CancellationToken cancellationToken)
    {
        return new List<HRDto>();
    }
}

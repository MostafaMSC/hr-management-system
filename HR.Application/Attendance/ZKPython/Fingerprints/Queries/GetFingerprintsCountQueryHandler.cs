using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.UserDevices.Queries;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.UserDevices.Queries;

public class GetHRsCountQueryHandler : IRequestHandler<GetHRsCountQuery, int>
{
    private readonly IFingerprintRepository _HRRepository;

    public GetHRsCountQueryHandler(IFingerprintRepository HRRepository)
    {
        _HRRepository = HRRepository;
    }

    public async Task<int> Handle(GetHRsCountQuery request, CancellationToken cancellationToken)
    {
        return 0;
    }
}

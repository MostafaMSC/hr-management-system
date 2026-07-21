using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Reports.Queries;

public record ExportAttendanceFilteredQuery(string? DeviceIp, string? DateFrom, string? DateTo, string? Search, int? DepartmentId = null) : IRequest<IAsyncEnumerable<AttendanceLogReportResultDto>>;

public class ExportAttendanceFilteredQueryHandler : IRequestHandler<ExportAttendanceFilteredQuery, IAsyncEnumerable<AttendanceLogReportResultDto>>
{
    private readonly IAttendanceLogRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ExportAttendanceFilteredQueryHandler(IAttendanceLogRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public Task<IAsyncEnumerable<AttendanceLogReportResultDto>> Handle(ExportAttendanceFilteredQuery request, CancellationToken cancellationToken)
    {
        DateTime? dtFrom = DateTime.TryParse(request.DateFrom, out DateTime f) ? DateTime.SpecifyKind(f, DateTimeKind.Utc) : null;
        DateTime? dtTo = DateTime.TryParse(request.DateTo, out DateTime t) ? DateTime.SpecifyKind(t, DateTimeKind.Utc) : null;

        var role = _currentUserService.Role;
        int? filterUserId = null;
        int? filterDeptId = null;

        if (role == Domain.Enums.UserType.Manager)
        {
            filterDeptId = _currentUserService.DepartmentId;
        }
        else if (role == Domain.Enums.UserType.Administrator)
        {
            filterDeptId = request.DepartmentId;
        }
        else if (role == Domain.Enums.UserType.Employee)
        {
            filterUserId = _currentUserService.UserId;
        }

        var stream = _repository.GetAttendanceReportStreamAsync(
            request.DeviceIp,
            dtFrom,
            dtTo,
            request.Search,
            filterUserId,
            filterDeptId);

        return Task.FromResult(MapStream(stream));
    }

    private async IAsyncEnumerable<AttendanceLogReportResultDto> MapStream(IAsyncEnumerable<AttendanceLogReportDto> stream)
    {
        await foreach (var x in stream)
        {
            yield return new AttendanceLogReportResultDto
            {
                UserID = x.UserID,
                Name = x.Name,
                Department = x.Department,
                Date = x.Date.ToString("yyyy-MM-dd"),
                CheckIn = x.CheckIn.ToString("HH:mm"),
                CheckOut = (x.CheckIn == x.CheckOut || (x.CheckOut - x.CheckIn).TotalMinutes < 60)
                    ? null
                    : x.CheckOut.ToString("HH:mm")
            };
        }
    }
}

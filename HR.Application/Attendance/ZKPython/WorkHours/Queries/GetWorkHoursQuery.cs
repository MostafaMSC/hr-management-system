using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.WorkHours.DTOs;

namespace HR.Application.Attendance.ZKPython.WorkHours.Queries;

public record GetWorkHoursQuery(
    string Time = "08:30",
    string FinishTime = "16:00",
    double RequiredDailyHours = 8,
    int WorkingDaysPerMonth = 26,
    string? DeviceIp = null
) : IRequest<List<WorkHoursDto>>;

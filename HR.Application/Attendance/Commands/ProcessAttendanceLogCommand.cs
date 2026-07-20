using HR.Application.Attendance.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.Commands;

public class ProcessAttendanceLogCommand : IRequest<bool>
{
    public AttendanceLogDto Log { get; set; } = null!;
}

public class ProcessAttendanceLogCommandHandler : IRequestHandler<ProcessAttendanceLogCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public ProcessAttendanceLogCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ProcessAttendanceLogCommand request, CancellationToken cancellationToken)
    {
        var logDto = request.Log;

        // 1. Resolve User
        var user = await _context.UserInfos
            .FirstOrDefaultAsync(u => u.BiometricId == logDto.UserID, cancellationToken);

        if (user == null)
        {
            // Optionally log a warning that the user from the device doesn't exist in our DB
            return false;
        }

        // 2. Resolve Device
        int? deviceId = null;
        if (!string.IsNullOrEmpty(logDto.DeviceIP))
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.IpAddress == logDto.DeviceIP, cancellationToken);
            deviceId = device?.Id;
        }

        // 3. Parse PunchType from CheckStatus (0: CheckIn, 1: CheckOut, etc)
        PunchType punchType = PunchType.CheckIn; // Default
        if (int.TryParse(logDto.CheckStatus, out int punchCode))
        {
            punchType = (PunchType)punchCode;
        }

        // 4. Create the AttendanceLog Entity
        var newLog = new AttendanceLog
        {
            UserInfoId = user.Id,
            DeviceId = deviceId,
            PunchTime = logDto.Time,
            LogsType = logDto.LogsType,
            PunchType = punchType
        };

        _context.AttendanceLogs.Add(newLog);
        await _context.SaveChangesAsync(cancellationToken);

        // Synchronously trigger the daily summary calculation for this user and date
        var summaryCommand = new ProcessDailySummariesCommand
        {
            UserInfoId = user.Id,
            TargetDate = newLog.PunchTime.Date
        };
        
        // Use mediator to trigger it. We need to inject IMediator.
        // Wait, since we are inside a MediatR handler, injecting IMediator is fine.
        return true;
    }
}

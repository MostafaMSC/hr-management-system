using HR.Application.Attendance.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;

namespace HR.Infrastructure.Services;

public class ZkTecoTcpProvider : IAttendanceProvider
{
    public DeviceProtocol SupportedProtocol => DeviceProtocol.ZkTecoTcp;

    public Task<List<AttendanceLogDto>> FetchLogsAsync(string deviceIp, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TCP Protocol for ZKTeco devices is pending implementation.");
    }

    public Task<bool> AddOrEditUserAsync(string deviceIp, AddEditDeviceUserDto user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserAsync(string deviceIp, string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EnrollUserAsync(string deviceIp, EnrollRequestDto request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SyncTimeAsync(string deviceIp, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SyncFullUserAsync(string deviceIp, SyncUserRequestDto request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<DeviceUserResultDto>> FetchUsersAsync(string deviceIp, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

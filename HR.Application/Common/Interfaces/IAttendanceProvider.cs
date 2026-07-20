using HR.Application.Attendance.DTOs;

namespace HR.Application.Common.Interfaces;

public interface IAttendanceProvider
{
    HR.Domain.Enums.DeviceProtocol SupportedProtocol { get; }
    Task<List<AttendanceLogDto>> FetchLogsAsync(string deviceIp, CancellationToken cancellationToken);
    
    Task<bool> AddOrEditUserAsync(string deviceIp, AddEditDeviceUserDto user, CancellationToken cancellationToken);
    
    Task<bool> DeleteUserAsync(string deviceIp, string userId, CancellationToken cancellationToken);
    
    Task<bool> EnrollUserAsync(string deviceIp, EnrollRequestDto request, CancellationToken cancellationToken);
    
    Task<bool> SyncTimeAsync(string deviceIp, CancellationToken cancellationToken);
    
    Task<bool> SyncFullUserAsync(string deviceIp, SyncUserRequestDto request, CancellationToken cancellationToken);
    
    Task<List<DeviceUserResultDto>> FetchUsersAsync(string deviceIp, CancellationToken cancellationToken);
}

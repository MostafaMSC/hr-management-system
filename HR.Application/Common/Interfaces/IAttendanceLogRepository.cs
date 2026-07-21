using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Application.Attendance.ZKPython.DTOs;

namespace HR.Application.Common.Interfaces;

public interface IAttendanceLogRepository
{
    Task<List<AttendanceLog>> SearchByNameAsync(string name);
    Task<List<AttendanceLog>> GetLogsByUserIdAsync(string userId, string? deviceIp = null);
    Task<List<AttendanceLog>> GetTodayLogsAsync(string? deviceIp = null);
    Task<int> GetLogsCountAsync(string? deviceIp = null);
    Task<(List<AttendanceLog> Data, int Total)> GetPagedLogsAsync(int page, int pageSize, string? deviceIp, int? userId = null, int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null, string? employeeId = null, CancellationToken cancellationToken = default);
    Task<List<AttendanceLog>> GetAllLogsAsync(string? deviceIp = null, int? userId = null, int? departmentId = null);
    IAsyncEnumerable<AttendanceLog> GetAllLogsStreamAsync(string? deviceIp = null, int? userId = null, int? departmentId = null, DateTime? dateFrom = null, DateTime? dateTo = null);

    // For Weekly Reports
    Task<List<AttendanceLog>> GetWeeklyLogsForUserAsync(string userId, DateTime weekStart, DateTime weekEnd);


    // For Filtered Export / Report
    Task<(List<AttendanceLogReportDto> data, int total)> GetAttendanceReportAsync(
        int page, int pageSize, string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search,
        int? userId = null, int? departmentId = null);

    Task<List<AttendanceLogReportDto>> GetAttendanceReportFilteredAsync(
        string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search,
        int? userId = null, int? departmentId = null);

    IAsyncEnumerable<AttendanceLogReportDto> GetAttendanceReportStreamAsync(
        string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search,
        int? userId = null, int? departmentId = null);

    // For Late Report
    Task<List<AttendanceLog>> GetLateLogsAsync(TimeSpan lateTime, DateTime date, string? deviceIp = null);
    Task<List<AttendanceLog>> GetLogsForPeriodAsync(DateTime from, DateTime to, string? deviceIp = null);


    // Common Queries
    Task<List<string>> GetCardsAsync();
    Task<List<string?>> GetRolesAsync();

    // Core methods
    Task AddRangeAsync(IEnumerable<AttendanceLog> logs);
    Task<bool> ExistsAsync(string userId, DateTime time);

    // For resync - delete all logs for a device
    Task<int> DeleteByDeviceAsync(string deviceIp);
}

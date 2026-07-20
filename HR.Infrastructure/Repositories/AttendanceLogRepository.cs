using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;

namespace HR.Infrastructure.Repositories
{
    public class AttendanceLogRepository : IAttendanceLogRepository
    {
        public Task<List<AttendanceLog>> SearchByNameAsync(string name) => Task.FromResult(new List<AttendanceLog>());
        public Task<List<AttendanceLog>> GetLogsByUserIdAsync(string userId, string? deviceIp = null) => Task.FromResult(new List<AttendanceLog>());
        public Task<List<AttendanceLog>> GetTodayLogsAsync(string? deviceIp = null) => Task.FromResult(new List<AttendanceLog>());
        public Task<int> GetLogsCountAsync(string? deviceIp = null) => Task.FromResult(0);
        public Task<(List<AttendanceLog> Data, int Total)> GetPagedLogsAsync(int page, int pageSize, string? deviceIp, int? userId = null, int? departmentId = null, CancellationToken cancellationToken = default) => Task.FromResult((new List<AttendanceLog>(), 0));
        public Task<List<AttendanceLog>> GetAllLogsAsync(string? deviceIp = null, int? userId = null, int? departmentId = null) => Task.FromResult(new List<AttendanceLog>());

#pragma warning disable CS1998
        public async IAsyncEnumerable<AttendanceLog> GetAllLogsStreamAsync(string? deviceIp = null, int? userId = null, int? departmentId = null, DateTime? dateFrom = null, DateTime? dateTo = null) { yield break; }
#pragma warning restore CS1998

        public Task<List<AttendanceLog>> GetWeeklyLogsForUserAsync(string userId, DateTime weekStart, DateTime weekEnd) => Task.FromResult(new List<AttendanceLog>());

        public Task<(List<AttendanceLogReportDto> data, int total)> GetAttendanceReportAsync(int page, int pageSize, string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search, int? userId = null, int? departmentId = null) => Task.FromResult((new List<AttendanceLogReportDto>(), 0));
        public Task<List<AttendanceLogReportDto>> GetAttendanceReportFilteredAsync(string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search, int? userId = null, int? departmentId = null) => Task.FromResult(new List<AttendanceLogReportDto>());

#pragma warning disable CS1998
        public async IAsyncEnumerable<AttendanceLogReportDto> GetAttendanceReportStreamAsync(string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search, int? userId = null, int? departmentId = null) { yield break; }
#pragma warning restore CS1998

        public Task<List<AttendanceLog>> GetLateLogsAsync(TimeSpan lateTime, DateTime date, string? deviceIp = null) => Task.FromResult(new List<AttendanceLog>());
        public Task<List<AttendanceLog>> GetLogsForPeriodAsync(DateTime from, DateTime to, string? deviceIp = null) => Task.FromResult(new List<AttendanceLog>());

        public Task<List<string>> GetCardsAsync() => Task.FromResult(new List<string>());
        public Task<List<string?>> GetRolesAsync() => Task.FromResult(new List<string?>());
        public Task AddRangeAsync(IEnumerable<AttendanceLog> logs) => Task.CompletedTask;
        public Task<bool> ExistsAsync(string userId, DateTime time) => Task.FromResult(false);
        public Task<int> DeleteByDeviceAsync(string deviceIp) => Task.FromResult(0);
    }
}

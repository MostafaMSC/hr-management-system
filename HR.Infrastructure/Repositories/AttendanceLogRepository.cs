using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Repositories
{
    public class AttendanceLogRepository : IAttendanceLogRepository
    {
        private readonly IApplicationDbContext _context;

        public AttendanceLogRepository(IApplicationDbContext context)
        {
            _context = context;
        }
        public Task<List<AttendanceLog>> SearchByNameAsync(string name) => 
            _context.AttendanceLogs.Include(l => l.UserInfo).Where(l => l.UserInfo.FirstName.Contains(name) || l.UserInfo.LastName.Contains(name)).ToListAsync();
            
        public Task<List<AttendanceLog>> GetLogsByUserIdAsync(string userId, string? deviceIp = null) => 
            _context.AttendanceLogs.Where(l => l.UserInfo.BiometricId == userId && (deviceIp == null || l.DeviceIP == deviceIp)).ToListAsync();
            
        public Task<List<AttendanceLog>> GetTodayLogsAsync(string? deviceIp = null) => 
            _context.AttendanceLogs.Where(l => l.PunchTime.Date == DateTime.UtcNow.Date && (deviceIp == null || l.DeviceIP == deviceIp)).ToListAsync();
            
        public Task<int> GetLogsCountAsync(string? deviceIp = null) => 
            _context.AttendanceLogs.CountAsync(l => deviceIp == null || l.DeviceIP == deviceIp);
            
        public async Task<(List<AttendanceLog> Data, int Total)> GetPagedLogsAsync(int page, int pageSize, string? deviceIp, int? userId = null, int? departmentId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.AttendanceLogs.Include(l => l.UserInfo).AsQueryable();
            
            if (!string.IsNullOrEmpty(deviceIp)) query = query.Where(l => l.DeviceIP == deviceIp);
            if (userId.HasValue) query = query.Where(l => l.UserInfoId == userId.Value);
            if (departmentId.HasValue) query = query.Where(l => l.UserInfo.DepartmentId == departmentId.Value);

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(l => l.PunchTime)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync(cancellationToken);
            return (data, total);
        }
        
        public Task<List<AttendanceLog>> GetAllLogsAsync(string? deviceIp = null, int? userId = null, int? departmentId = null)
        {
            var query = _context.AttendanceLogs.Include(l => l.UserInfo).AsQueryable();
            if (!string.IsNullOrEmpty(deviceIp)) query = query.Where(l => l.DeviceIP == deviceIp);
            if (userId.HasValue) query = query.Where(l => l.UserInfoId == userId.Value);
            if (departmentId.HasValue) query = query.Where(l => l.UserInfo.DepartmentId == departmentId.Value);
            return query.ToListAsync();
        }

#pragma warning disable CS1998
        public async IAsyncEnumerable<AttendanceLog> GetAllLogsStreamAsync(string? deviceIp = null, int? userId = null, int? departmentId = null, DateTime? dateFrom = null, DateTime? dateTo = null) { yield break; }
#pragma warning restore CS1998

        public Task<List<AttendanceLog>> GetWeeklyLogsForUserAsync(string userId, DateTime weekStart, DateTime weekEnd) => 
            _context.AttendanceLogs.Where(l => l.UserInfo.BiometricId == userId && l.PunchTime >= weekStart && l.PunchTime <= weekEnd).ToListAsync();

        public async Task<(List<AttendanceLogReportDto> data, int total)> GetAttendanceReportAsync(int page, int pageSize, string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search, int? userId = null, int? departmentId = null)
        {
            var query = _context.AttendanceLogs.Include(l => l.UserInfo).ThenInclude(u => u.Department).AsQueryable();
            
            if (!string.IsNullOrEmpty(deviceIp)) query = query.Where(l => l.DeviceIP == deviceIp);
            if (dateFrom.HasValue) query = query.Where(l => l.PunchTime >= dateFrom.Value);
            if (dateTo.HasValue) query = query.Where(l => l.PunchTime <= dateTo.Value);
            if (!string.IsNullOrEmpty(search)) query = query.Where(l => l.UserInfo.FirstName.Contains(search) || l.UserInfo.LastName.Contains(search));
            if (userId.HasValue) query = query.Where(l => l.UserInfoId == userId.Value);
            if (departmentId.HasValue) query = query.Where(l => l.UserInfo.DepartmentId == departmentId.Value);

            var grouped = query.GroupBy(l => new { l.UserInfoId, l.PunchTime.Date });
            var total = await grouped.CountAsync();
            
            var paginatedGroups = await grouped
                                  .Select(g => new { 
                                      UserId = g.Key.UserInfoId, 
                                      Date = g.Key.Date, 
                                      CheckIn = g.Min(x => x.PunchTime), 
                                      CheckOut = g.Max(x => x.PunchTime) 
                                  })
                                  .OrderByDescending(g => g.Date)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();
                                  
            var userIds = paginatedGroups.Select(g => g.UserId).Distinct().ToList();
            var users = await _context.UserInfos.Include(u => u.Department).Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);
                                  
            var data = paginatedGroups.Select(g => new AttendanceLogReportDto
            {
                UserID = users[g.UserId].BiometricId ?? g.UserId.ToString(),
                Name = $"{users[g.UserId].FirstName} {users[g.UserId].LastName}",
                Department = users[g.UserId].Department?.Name,
                Date = g.Date,
                CheckIn = g.CheckIn,
                CheckOut = g.CheckOut
            }).ToList();
            
            return (data, total);
        }
        
        public async Task<List<AttendanceLogReportDto>> GetAttendanceReportFilteredAsync(string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search, int? userId = null, int? departmentId = null)
        {
            var result = await GetAttendanceReportAsync(1, int.MaxValue, deviceIp, dateFrom, dateTo, search, userId, departmentId);
            return result.data;
        }

#pragma warning disable CS1998
        public async IAsyncEnumerable<AttendanceLogReportDto> GetAttendanceReportStreamAsync(string? deviceIp, DateTime? dateFrom, DateTime? dateTo, string? search, int? userId = null, int? departmentId = null) { yield break; }
#pragma warning restore CS1998

        public Task<List<AttendanceLog>> GetLateLogsAsync(TimeSpan lateTime, DateTime date, string? deviceIp = null) => 
            _context.AttendanceLogs.Where(l => l.PunchTime.Date == date.Date && l.PunchTime.TimeOfDay > lateTime && (deviceIp == null || l.DeviceIP == deviceIp)).ToListAsync();
            
        public Task<List<AttendanceLog>> GetLogsForPeriodAsync(DateTime from, DateTime to, string? deviceIp = null) => 
            _context.AttendanceLogs.Where(l => l.PunchTime >= from && l.PunchTime <= to && (deviceIp == null || l.DeviceIP == deviceIp)).ToListAsync();

        public Task<List<string>> GetCardsAsync() => Task.FromResult(new List<string>());
        public Task<List<string?>> GetRolesAsync() => Task.FromResult(new List<string?>());
        
        public async Task AddRangeAsync(IEnumerable<AttendanceLog> logs)
        {
            _context.AttendanceLogs.AddRange(logs);
            await _context.SaveChangesAsync(default);
        }
        
        public Task<bool> ExistsAsync(string userId, DateTime time) => 
            _context.AttendanceLogs.AnyAsync(l => l.UserInfo.BiometricId == userId && l.PunchTime == time);
            
        public async Task<int> DeleteByDeviceAsync(string deviceIp)
        {
            var logs = await _context.AttendanceLogs.Where(l => l.DeviceIP == deviceIp).ToListAsync();
            _context.AttendanceLogs.RemoveRange(logs);
            return await _context.SaveChangesAsync(default);
        }
    }
}

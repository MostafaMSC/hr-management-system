using System;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using HR.Application.Attendance.ZKPython.WorkHours.Queries;
using HR.Application.Attendance.ZKPython.Users.Queries;
using HR.Application.Attendance.ZKPython.Logs.Queries;
using HR.Application.Attendance.ZKPython.Users.Commands;
using HR.Application.Attendance.ZKPython.Logs.Commands;
using HR.Application.Attendance.ZKPython.Reports.Queries;
using HR.Application.Attendance.ZKPython.Common.Queries;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.UserDevices.Commands;
using HR.Application.Attendance.ZKPython.UserDevices.Queries;
using HR.Application.Common.Interfaces;

namespace HR.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ZKPythonController : ControllerBase
    {
        private readonly ILogger<ZKPythonController> _logger;
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;

        public ZKPythonController(
            ILogger<ZKPythonController> logger, 
            IMediator mediator,
            IWebHostEnvironment environment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [HttpGet("get-debug-info")]
        public async Task<IActionResult> GetDebugInfo([FromQuery] string userId)
        {
            var result = await _mediator.Send(new GetDebugInfoQuery(userId));
            return Ok(result);
        }

        [HttpGet("get-all-fingerprints")]
        public async Task<IActionResult> GetAllFingerprints(CancellationToken cancellationToken = default)
        {
            var fingerprints = await _mediator.Send(new GetAllHRsQuery(), cancellationToken);
            return Ok(new { success = true, count = fingerprints.Count, fingerprints });
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 100, [FromQuery] string deviceIp = null)
        {
            var result = await _mediator.Send(new GetLogsQuery(page, pageSize, deviceIp));
            return Ok(new { success = true, total = result.Total, page = result.Page, pageSize = result.PageSize, count = result.Data.Count, data = result.Data });
        }

        [HttpGet("get-logs_from_device")]
        public async Task<IActionResult> GetLogsByDevice([FromQuery] int page = 1, [FromQuery] int pageSize = 100, [FromQuery] string deviceIp = null, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetLogsByDeviceQuery(page, pageSize, deviceIp));
            return Ok(new { success = result.Success, total = result.Total, page = result.Page, pageSize = result.PageSize, count = result.Count, data = result.Data });
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers([FromQuery] string deviceIp = null)
        {
            var users = await _mediator.Send(new GetUsersQuery(deviceIp));
            return Ok(new { success = true, count = users.Count, users });
        }

        [HttpGet("get-users-from-device")]
        public async Task<IActionResult> GetUsersFromDevice([FromQuery] string deviceIp = null)
        {
            var result = await _mediator.Send(new GetUsersFromDeviceQuery(deviceIp));
            if (result.Success) return Ok(new { success = true, count = result.Users.Count, users = result.Users });
            return BadRequest(new { success = false, message = result.ErrorDetail });
        }

        [HttpGet("get-user/{id}")]
        public async Task<IActionResult> GetUserLogs(string id, [FromQuery] string deviceIp = null)
        {
            var userLogs = await _mediator.Send(new GetUserLogsQuery(id, deviceIp));
            return Ok(new { success = true, count = userLogs.Count, userLogs });
        }

        [HttpGet("get-count")]
        public async Task<IActionResult> GetLogsCount([FromQuery] string deviceIp = null)
        {
            var result = await _mediator.Send(new GetLogsQuery(1, 1, deviceIp));
            return Ok(new { success = true, count = result.Total });
        }

        [HttpGet("get-users-count")]
        public async Task<IActionResult> GetUsersCount([FromQuery] string deviceIp = null)
        {
            var users = await _mediator.Send(new GetUsersQuery(deviceIp));
            return Ok(new { success = true, count = users.Count });
        }

        [HttpGet("get-today")]
        public async Task<IActionResult> GetTodayLogs([FromQuery] string deviceIp = null)
        {
            var todayLogs = await _mediator.Send(new GetTodayLogsQuery(deviceIp));
            return Ok(new { success = true, count = todayLogs.Count, logs = todayLogs });
        }

        [HttpGet("get-users-count-by-department")]
        public async Task<IActionResult> GetUsersCountByDepartment()
        {
            var users = await _mediator.Send(new GetUsersQuery(null));
            var grouped = users
                .GroupBy(u => u.Department?.Name ?? "Unassigned")
                .Select(g => new { department = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToList();
            
            return Ok(new { success = true, data = grouped });
        }

        [HttpPost("sync-users")]
        public async Task<IActionResult> SyncUsers([FromQuery] string? deviceIp)
        {
            var result = await _mediator.Send(new SyncUsersCommand(deviceIp));
            if (result.Success) return Ok(new { success = true, message = result.Message });
            return BadRequest(new { success = false, message = result.ErrorDetail });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            var results = await _mediator.Send(new SearchLogsQuery(name));
            return Ok(new { success = true, results });
        }

        [HttpGet("get-late")]
        public async Task<IActionResult> GetLate([FromQuery] string time = "08:30", [FromQuery] string deviceIp = null)
        {
            var lateLogs = await _mediator.Send(new GetLateQuery(time, deviceIp));
            return Ok(new { success = true, count = lateLogs.Count, lateLogs });
        }

        [HttpGet("get-weekly-late")]
        public async Task<IActionResult> GetWeeklyLate([FromQuery] string time = "08:30", [FromQuery] string deviceIp = null)
        {
            var result = await _mediator.Send(new GetWeeklyLateQuery(time, deviceIp));
            return Ok(new { success = true, weekStart = result.WeekStart, weekEnd = result.WeekEnd, requiredTime = result.RequiredTime, result = result.Result });
        }

        [HttpGet("get-work-hours")]
        public async Task<IActionResult> GetWorkHours(
            [FromQuery] string time = "08:30",
            [FromQuery] string FinishTime = "16:00",
            [FromQuery] double requiredDailyHours = 8,
            [FromQuery] int workingDaysPerMonth = 26,
            [FromQuery] string deviceIp = null)
        {
            var query = new GetWorkHoursQuery(time, FinishTime, requiredDailyHours, workingDaysPerMonth, deviceIp);
            var result = await _mediator.Send(query);
            return Ok(new { success = true, result });
        }

        [HttpPost("sync-logs")]
        public async Task<IActionResult> SyncLogs([FromQuery] string? deviceIp)
        {
            var result = await _mediator.Send(new SyncLogsCommand(deviceIp));
            return Ok(new { success = true, message = result.Message, added = result.Added, skipped = result.Skipped, total = result.Total });
        }

        [HttpPost("sync-time")]
        public async Task<IActionResult> SyncTime([FromQuery] string deviceIp)
        {
            var result = await _mediator.Send(new SyncTimeCommand(deviceIp));
            if (result.Success) return Ok(new { success = true, message = result.Message });
            return BadRequest(new { success = false, message = result.ErrorDetail });
        }

        [HttpPost("resync-logs")]
        public async Task<IActionResult> ResyncLogs([FromQuery] string deviceIp)
        {
            var result = await _mediator.Send(new ResyncLogsCommand(deviceIp));
            if (result.Success) return Ok(new { success = true, message = result.Message, deletedCount = result.DeletedCount, addedCount = result.AddedCount });
            return BadRequest(new { success = false, message = result.ErrorDetail });
        }

        [HttpGet("get-attendance-report")]
        public async Task<IActionResult> GetAttendanceReport(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string deviceIp = null,
            [FromQuery] string dateFrom = null,
            [FromQuery] string dateTo = null,
            [FromQuery] string search = null,
            [FromQuery] int? departmentId = null)
        {
            var result = await _mediator.Send(new GetAttendanceReportQuery(page, pageSize, deviceIp, dateFrom, dateTo, search, departmentId));
            return Ok(new { success = true, page = result.Page, pageSize = result.PageSize, total = result.Total, totalPages = result.TotalPages, data = result.Data });
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest  req, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new AddUserCommand(req), cancellationToken);
            if (result.Success) return Ok(new { success = true, result.Message, result.User });
            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost("edit-user")]
        public async Task<IActionResult> EditUser([FromBody] AddUserRequest req, [FromQuery] string userId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new EditUserCommand(userId, req), cancellationToken);
            if (result.Success) return Ok(new { success = true, result.Message });
            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost("delete-user")]
        public async Task<IActionResult> DeleteUser([FromQuery] string deviceIp, [FromQuery] string userId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteUserCommand(deviceIp, userId), cancellationToken);
            if (result.Success) return Ok(new { success = true, result.Message });
            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost("sync-fingerprints")]
        public async Task<IActionResult> SyncFingerprints([FromQuery] string? deviceIp, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new SyncHRsCommand(deviceIp), cancellationToken);
            if (result.Success) return Ok(new { success = true, message = result.Message, added = result.Added, updated = result.Updated, skipped = result.Skipped, total = result.Total });
            return BadRequest(new { success = false, message = result.ErrorDetail });
        }

        [HttpGet("get-user-fingerprints/{userId}")]
        public async Task<IActionResult> GetUserFingerprints(int userId, CancellationToken cancellationToken = default)
        {
            var fingerprints = await _mediator.Send(new GetUserHRsQuery(userId), cancellationToken);
            return Ok(new { success = true, count = fingerprints.Count, fingerprints });
        }

        [HttpPost("enroll-user")]
        public async Task<IActionResult> EnrollUser([FromQuery] string deviceIp, [FromQuery] string userId, [FromQuery] int fingerIndex = 0, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new EnrollUserCommand(deviceIp, userId, fingerIndex), cancellationToken);
            if (result.Success) return Ok(new { success = true, Message = result.Message });
            return BadRequest(new { success = false, message = result.Message });
        }
    }
}

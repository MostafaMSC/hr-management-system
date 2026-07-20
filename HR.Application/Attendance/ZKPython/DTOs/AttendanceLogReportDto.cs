using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
namespace HR.Application.Attendance.ZKPython.DTOs
{
    public class AttendanceLogReportDto
    {
        public string UserID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Department { get; set; }
        public DateTime Date { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }
}

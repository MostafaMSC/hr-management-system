using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            return SendEmailAsync(to, subject, body, false, CancellationToken.None, null);
        }

        public Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default, IEnumerable<EmailAttachment>? attachments = null)
        {
            _logger.LogInformation("Dummy EmailService: Sending email to {To} with Subject {Subject}. Attachments count: {Count}",
                to, subject, attachments?.Count() ?? 0);

            // Dummy implementation for now
            return Task.CompletedTask;
        }
        public async Task SendWeeklyAttendanceReportAsync(
            string toEmail,
            string employeeName,
            System.Collections.Generic.List<HR.Domain.Entities.AttendanceLog> weeklyLogs,
            DateTime weekStart,
            DateTime weekEnd,
            CancellationToken cancellationToken = default)
        {
            // Group logs by date and calculate daily hours
            // Required work time: 8:30 AM to 4:00 PM
            var requiredStartTime = new TimeSpan(8, 30, 0); // 8:30 AM
            
            var dailyRecords = weeklyLogs
                .GroupBy(l => l.Time.Date)
                .Select(g => 
                {
                    var checkIn = g.Min(l => l.Time);
                    var checkOut = g.Max(l => l.Time);
                    
                    // Calculate late arrival in minutes
                    var lateMinutes = 0;
                    if (checkIn.TimeOfDay > requiredStartTime)
                    {
                        lateMinutes = (int)(checkIn.TimeOfDay - requiredStartTime).TotalMinutes;
                    }
                    
                    return new
                    {
                        Date = g.Key,
                        CheckIn = checkIn,
                        CheckOut = checkOut,
                        TotalHours = (checkOut - checkIn).TotalHours,
                        LateMinutes = lateMinutes
                    };
                })
                .OrderBy(r => r.Date)
                .ToList();

            var totalWeeklyHours = dailyRecords.Sum(r => r.TotalHours);
            var daysWorked = dailyRecords.Count;
            var totalLateDays = dailyRecords.Count(r => r.LateMinutes > 0);

            // Build table rows with late arrival column
            var tableRows = string.Join("", dailyRecords.Select((r, index) =>
            {
                var bgColor = index % 2 == 0 ? "#f3f4f6" : "#ffffff";
                var lateDisplay = r.LateMinutes > 0 
                    ? $"<span style='color: #dc2626; font-weight: bold;'>{r.LateMinutes} min</span>" 
                    : "<span style='color: #10b981;'>On Time ✓</span>";
                    
                return $@"
                    <tr style='background: {bgColor};'>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{r.Date:dd/MM/yyyy (dddd)}</td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{r.CheckIn:HH:mm}</td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{r.CheckOut:HH:mm}</td>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>{r.TotalHours:F2} hours</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd; text-align: center;'>{lateDisplay}</td>
                    </tr>";
            }));

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; background: #ffffff; border: 1px solid #e5e7eb; border-radius: 10px; overflow: hidden;'>
                    <!-- Header -->
                    <div style='background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); padding: 30px; text-align: center;'>
                        <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>📊 Weekly Attendance Report</h1>
                        <p style='color: #e0e7ff; margin: 10px 0 0 0; font-size: 14px;'>
                            {weekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}
                        </p>
                    </div>

                    <!-- Body -->
                    <div style='padding: 30px;'>
                        <p style='font-size: 16px; color: #374151; margin: 0 0 20px 0;'>
                            Dear <strong>{employeeName}</strong>,
                        </p>
                        <p style='font-size: 14px; color: #6b7280; margin: 0 0 25px 0;'>
                            Here's your attendance summary for the week. Keep up the great work! 🌟
                        </p>

                        <!-- Summary Cards -->
                        <div style='display: flex; gap: 15px; margin-bottom: 30px;'>
                            <div style='flex: 1; background: #eff6ff; border-left: 4px solid #2563eb; padding: 15px; border-radius: 6px;'>
                                <div style='color: #1e40af; font-size: 12px; text-transform: uppercase; font-weight: 600;'>Days Worked</div>
                                <div style='color: #1e3a8a; font-size: 24px; font-weight: bold; margin-top: 5px;'>{daysWorked}</div>
                            </div>
                            <div style='flex: 1; background: #f0fdf4; border-left: 4px solid #10b981; padding: 15px; border-radius: 6px;'>
                                <div style='color: #047857; font-size: 12px; text-transform: uppercase; font-weight: 600;'>Total Hours</div>
                                <div style='color: #065f46; font-size: 24px; font-weight: bold; margin-top: 5px;'>{totalWeeklyHours:F1}h</div>
                            </div>
                            <div style='flex: 1; background: {(totalLateDays > 0 ? "#fef2f2" : "#f0fdf4")}; border-left: 4px solid {(totalLateDays > 0 ? "#ef4444" : "#10b981")}; padding: 15px; border-radius: 6px;'>
                                <div style='color: {(totalLateDays > 0 ? "#991b1b" : "#047857")}; font-size: 12px; text-transform: uppercase; font-weight: 600;'>Late Days</div>
                                <div style='color: {(totalLateDays > 0 ? "#7f1d1d" : "#065f46")}; font-size: 24px; font-weight: bold; margin-top: 5px;'>{totalLateDays}</div>
                            </div>
                        </div>

                        <!-- Detailed Table -->
                        <h3 style='color: #111827; margin: 0 0 15px 0; font-size: 18px;'>Daily Breakdown</h3>
                        <table style='width: 100%; border-collapse: collapse; margin: 0 0 25px 0; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>
                            <thead>
                                <tr style='background: #1f2937; color: #ffffff;'>
                                    <th style='padding: 12px; text-align: left; border: 1px solid #374151;'>Date</th>
                                    <th style='padding: 12px; text-align: left; border: 1px solid #374151;'>Check In</th>
                                    <th style='padding: 12px; text-align: left; border: 1px solid #374151;'>Check Out</th>
                                    <th style='padding: 12px; text-align: left; border: 1px solid #374151;'>Total Hours</th>
                                    <th style='padding: 12px; text-align: center; border: 1px solid #374151;'>Late Arrival</th>
                                </tr>
                            </thead>
                            <tbody>
                                {tableRows}
                            </tbody>
                        </table>

                        <div style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                            <p style='margin: 0; color: #92400e; font-size: 13px;'>
                                💡 <strong>Tip:</strong> This report is generated automatically every Thursday. If you notice any discrepancies, please contact HR.
                            </p>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div style='background: #f9fafb; padding: 20px; text-align: center; border-top: 1px solid #e5e7eb;'>
                        <p style='margin: 0; color: #6b7280; font-size: 12px;'>
                            HR Management System<br>
                            © 2026 All rights reserved
                        </p>
                    </div>
                </div>
            ";

            await SendEmailAsync(
                toEmail,
                $"📊 Your Weekly Attendance Report ({weekStart:dd/MM} - {weekEnd:dd/MM})",
                body,
                isHtml: true,
                cancellationToken
            );
        }
    }
}

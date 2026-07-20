using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Application.Payroll.DTOs;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HR.Infrastructure.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IEmailService _emailService;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<PayrollService> _logger;

        public PayrollService(IEmailService emailService, IApplicationDbContext context, ILogger<PayrollService> logger)
        {
            _emailService = emailService;
            _context = context;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ═══════════════════════════════════════════════════════════════
        //  STEP 1 — Preview: parse Excel + match users, return results
        // ═══════════════════════════════════════════════════════════════
        public async Task<SalarySheetPreviewResponse> PreviewSalarySheetAsync(Stream excelStream, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheet(1);

            var cols = DetectColumns(worksheet);
            var users = await _context.UserInfos.ToListAsync(cancellationToken);
            _logger.LogInformation("📋 Preview: {UserCount} users in DB", users.Count);

            int currentYear = DateTime.UtcNow.Year;
            int currentMonth = DateTime.UtcNow.Month;
            var approvedBonuses = await _context.BonusRequests
                .Where(b => b.Status == HR.Domain.Enums.BonusStatus.Approved 
                         && b.Year == currentYear 
                         && b.Month == currentMonth)
                .ToListAsync(cancellationToken);

            // Data starts at row after the header row
            var dataStartRow = cols.HeaderRow + 1;
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            var matches = new List<SalarySheetMatchDto>();
            int rowIdx = 0;

            for (int r = dataStartRow; r <= lastRow; r++)
            {
                var row = worksheet.Row(r);
                rowIdx++;

                string fullName = row.Cell(cols.FullName).GetValue<string>()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(fullName)) continue;

                var dto = new SalarySheetMatchDto
                {
                    RowIndex = rowIdx,
                    ExcelName = fullName,
                    EmployeeId = SafeGet(row, cols.EmployeeId),
                    Location = SafeGet(row, cols.Location),
                    Status = SafeGet(row, cols.Status),
                    JobTitle = SafeGet(row, cols.JobTitle),
                    ReportingTo = SafeGet(row, cols.ReportingTo),
                    Department = SafeGet(row, cols.Department),
                    Section = SafeGet(row, cols.Section),
                    GrossSalary = SafeGet(row, cols.GrossSalary),
                    NetSalary = SafeGet(row, cols.NetSalary),
                    OverTime = SafeGet(row, cols.OverTime),
                    Mission = SafeGet(row, cols.Mission),
                    PlusDiscrepancies = SafeGet(row, cols.PlusDiscrepancies),
                    Bonus = SafeGet(row, cols.Bonus),
                    SS5 = SafeGet(row, cols.SS5),
                    MinusDiscrepancies = SafeGet(row, cols.MinusDiscrepancies),
                    DeptDeduction = SafeGet(row, cols.DeptDeduction),
                    Delay = SafeGet(row, cols.Delay)
                };

                // ── Try matching ──
                var (matchedUser, matchType) = FindBestMatch(fullName, users);
                if (matchedUser != null)
                {
                    dto.MatchedUser = new MatchedUserDto
                    {
                        Id = matchedUser.Id,
                        Username = matchedUser.Username,
                        Email = matchedUser.Email ?? ""
                    };
                    dto.MatchType = matchType;

                    // ── Inject Approved Bonuses ──
                    var userBonuses = approvedBonuses.Where(b => b.TargetUserId == matchedUser.Id).ToList();
                    if (userBonuses.Any())
                    {
                        decimal currentBonus = 0;
                        decimal.TryParse(dto.Bonus, out currentBonus);
                        
                        decimal gross = 0;
                        decimal.TryParse(dto.GrossSalary, out gross);
                        
                        decimal addedBonus = 0;
                        foreach (var b in userBonuses)
                        {
                            if (b.Type == HR.Domain.Enums.BonusType.Amount)
                                addedBonus += b.Value;
                            else if (b.Type == HR.Domain.Enums.BonusType.Percentage)
                                addedBonus += gross * (b.Value / 100m);
                        }

                        currentBonus += addedBonus;
                        dto.Bonus = currentBonus.ToString("0.##"); // Keep format simple
                        
                        // Update Net Salary accordingly
                        decimal currentNet = 0;
                        if (decimal.TryParse(dto.NetSalary, out currentNet))
                        {
                            currentNet += addedBonus;
                            dto.NetSalary = currentNet.ToString("0.##");
                        }
                    }
                }

                matches.Add(dto);
            }

            var allUserDtos = users
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .Select(u => new MatchedUserDto { Id = u.Id, Username = u.Username, Email = u.Email ?? "" })
                .OrderBy(u => u.Username)
                .ToList();

            _logger.LogInformation("📊 Preview complete: {Total} rows, {Matched} matched, {Unmatched} unmatched",
                matches.Count, matches.Count(m => m.MatchedUser != null), matches.Count(m => m.MatchedUser == null));

            return new SalarySheetPreviewResponse
            {
                Matches = matches,
                AllUsers = allUserDtos,
                TotalRows = matches.Count,
                MatchedRows = matches.Count(m => m.MatchedUser != null),
                UnmatchedRows = matches.Count(m => m.MatchedUser == null)
            };
        }

        // ═══════════════════════════════════════════════════════════════
        //  STEP 2 — Send: generate PDFs + send emails for confirmed list
        // ═══════════════════════════════════════════════════════════════
        public async Task<SendSalarySlipsResponse> SendSalarySlipsAsync(SendSalarySlipsRequest request, CancellationToken cancellationToken)
        {
            var response = new SendSalarySlipsResponse();
            var users = await _context.UserInfos.ToListAsync(cancellationToken);
            var userMap = users.ToDictionary(u => u.Id);
            var departments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in request.Entries)
            {
                response.TotalProcessed++;

                if (!userMap.TryGetValue(entry.UserId, out var user))
                {
                    response.Failed++;
                    response.Errors.Add($"User ID {entry.UserId} not found in database.");
                    continue;
                }

                // Trim the email to handle invisible whitespace from the DB
                var recipientEmail = user.Email?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    response.Failed++;
                    response.Errors.Add($"'{user.Username}' has no email address.");
                    continue;
                }

                try
                {
                    byte[] pdfBytes = GeneratePdfSlip(entry);

                    var attachment = new EmailAttachment
                    {
                        FileName = $"Salary_Slip_{DateTime.Now:yyyy_MM}.pdf",
                        Content = pdfBytes,
                        ContentType = "application/pdf"
                    };

                    _logger.LogInformation("📧 Sending salary slip to {Email} for '{Name}'...", recipientEmail, entry.ExcelName);

                    await _emailService.SendEmailAsync(
                        recipientEmail,
                        "Your Monthly Salary Slip",
                        $"Dear {entry.ExcelName},<br><br>Please find attached your salary slip for this month.<br><br>Regards,<br>HR Department",
                        isHtml: true,
                        cancellationToken: cancellationToken,
                        attachments: new List<EmailAttachment> { attachment }
                    );

                    response.EmailsSent++;
                    if (!string.IsNullOrWhiteSpace(entry.Department))
                        departments.Add(entry.Department.Trim());

                    _logger.LogInformation("✅ Salary slip sent to {Email}", recipientEmail);
                }
                catch (Exception ex)
                {
                    response.Failed++;
                    response.Errors.Add($"Failed to send to '{user.Username}' ({recipientEmail}): {ex.Message}");
                    _logger.LogError(ex, "❌ Failed to send salary slip to {Email}", recipientEmail);
                }
            }

            response.Departments = departments.OrderBy(d => d).ToList();

            _logger.LogInformation("📊 Send complete: {Sent}/{Total} emails sent, {Failed} failed",
                response.EmailsSent, response.TotalProcessed, response.Failed);

            return response;
        }

        // ═══════════════════════════════════════════════════════════════
        //  HR COPY — Generate combined multi-page PDF filtered by dept
        // ═══════════════════════════════════════════════════════════════
        public Task<byte[]> GenerateHrCopyAsync(SendSalarySlipsRequest request, string? departmentFilter, CancellationToken cancellationToken)
        {
            var entries = request.Entries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(departmentFilter))
            {
                entries = entries.Where(e =>
                    string.Equals(e.Department?.Trim(), departmentFilter.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            var filteredList = entries.ToList();

            if (filteredList.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No payslip entries found{(string.IsNullOrWhiteSpace(departmentFilter) ? "" : $" for department '{departmentFilter}'")}.");
            }

            _logger.LogInformation("📄 Generating HR copy: {Count} slips{Filter}",
                filteredList.Count,
                string.IsNullOrWhiteSpace(departmentFilter) ? " (all departments)" : $" (dept: {departmentFilter})");

            var document = Document.Create(container =>
            {
                foreach (var entry in filteredList)
                {
                    BuildPayslipPage(container, entry);
                }
            });

            var pdfBytes = document.GeneratePdf();
            return Task.FromResult(pdfBytes);
        }

        // ═══════════════════════════════════════════════════════════════
        //  Legacy one-shot method (kept for backward compatibility)
        // ═══════════════════════════════════════════════════════════════
        public async Task ProcessSalarySheetAsync(Stream excelStream, CancellationToken cancellationToken)
        {
            var ms = new MemoryStream();
            await excelStream.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            var preview = await PreviewSalarySheetAsync(ms, cancellationToken);

            var sendRequest = new SendSalarySlipsRequest
            {
                Entries = preview.Matches
                    .Where(m => m.MatchedUser != null)
                    .Select(m => new ConfirmedSlipEntry
                    {
                        UserId = m.MatchedUser!.Id,
                        ExcelName = m.ExcelName,
                        EmployeeId = m.EmployeeId,
                        Location = m.Location,
                        Status = m.Status,
                        JobTitle = m.JobTitle,
                        ReportingTo = m.ReportingTo,
                        Department = m.Department,
                        Section = m.Section,
                        GrossSalary = m.GrossSalary,
                        NetSalary = m.NetSalary,
                        OverTime = m.OverTime,
                        Mission = m.Mission,
                        PlusDiscrepancies = m.PlusDiscrepancies,
                        Bonus = m.Bonus,
                        SS5 = m.SS5,
                        MinusDiscrepancies = m.MinusDiscrepancies,
                        DeptDeduction = m.DeptDeduction,
                        Delay = m.Delay
                    })
                    .ToList()
            };

            await SendSalarySlipsAsync(sendRequest, cancellationToken);
        }

        // ═══════════════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════════════

        private static string SafeGet(IXLRow row, int colIndex)
        {
            if (colIndex <= 0) return "";
            return row.Cell(colIndex).GetValue<string>()?.Trim() ?? "";
        }

        private (UserInfo? user, string matchType) FindBestMatch(string excelName, List<UserInfo> users)
        {
            // 1. Exact username match
            var exact = users.FirstOrDefault(u =>
                string.Equals(u.Username, excelName, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return (exact, "exact");

            // 2. Exact email match
            var emailMatch = users.FirstOrDefault(u =>
                string.Equals(u.Email, excelName, StringComparison.OrdinalIgnoreCase));
            if (emailMatch != null) return (emailMatch, "exact");

            // 3. Partial / contains match
            var partial = users.FirstOrDefault(u =>
                u.Username != null && (
                    u.Username.Contains(excelName, StringComparison.OrdinalIgnoreCase) ||
                    excelName.Contains(u.Username, StringComparison.OrdinalIgnoreCase)
                ));
            if (partial != null) return (partial, "partial");

            return (null, "none");
        }

        // ── Column detection record ──
        private record ColumnIndices(
            int HeaderRow,
            int FullName, int EmployeeId, int Location, int Status,
            int JobTitle, int ReportingTo, int Department, int Section,
            int GrossSalary, int NetSalary,
            int OverTime, int Mission, int PlusDiscrepancies, int Bonus,
            int SS5, int MinusDiscrepancies, int DeptDeduction, int Delay);

        private ColumnIndices DetectColumns(IXLWorksheet worksheet)
        {
            // Detect header row: scan first 5 rows for one that contains "Full Name"
            int headerRow = 1;
            for (int r = 1; r <= 5; r++)
            {
                var row = worksheet.Row(r);
                var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
                for (int c = 1; c <= lastCol; c++)
                {
                    var val = row.Cell(c).GetValue<string>()?.Trim() ?? "";
                    if (val.Contains("Full Name", StringComparison.OrdinalIgnoreCase))
                    {
                        headerRow = r;
                        goto found;
                    }
                }
            }
        found:

            var headerRowData = worksheet.Row(headerRow);
            var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int c = 1; c <= lastColumn; c++)
            {
                var h = headerRowData.Cell(c).GetValue<string>()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(h)) map[h] = c;
            }

            _logger.LogInformation("📊 Header at row {Row}, detected {Count} columns: {Headers}",
                headerRow, map.Count, string.Join(", ", map.Select(kv => $"{kv.Key}(Col{kv.Value})")));

            return new ColumnIndices(
                HeaderRow: headerRow,
                FullName: FindCol(map, "Full Name", "Name", "Employee Name", "Employee") ?? 3,
                EmployeeId: FindCol(map, "Employee ID", "Emp ID", "ID") ?? 2,
                Location: FindCol(map, "Location", "Office") ?? 0,
                Status: FindCol(map, "Status") ?? 0,
                JobTitle: FindCol(map, "Job Title", "Position", "Title") ?? 0,
                ReportingTo: FindCol(map, "Reporting to", "Reporting To", "Manager") ?? 0,
                Department: FindCol(map, "Department", "Dept") ?? 0,
                Section: FindCol(map, "Section") ?? 0,
                GrossSalary: FindCol(map, "Gross Salary", "Gross") ?? 0,
                NetSalary: FindCol(map, "Net Salary", "Net") ?? 0,
                OverTime: FindCol(map, "Over time", "Overtime", "Over Time") ?? 0,
                Mission: FindCol(map, "Mission") ?? 0,
                PlusDiscrepancies: FindCol(map, "(+)Discrepancies", "+Discrepancies", "Plus Discrepancies") ?? 0,
                Bonus: FindCol(map, "Bouns", "Bonus", "Increase/Bonus", "Increase") ?? 0,
                SS5: FindCol(map, "SS(5%)", "SS 5%", "SS (5%)") ?? 0,
                MinusDiscrepancies: FindCol(map, "(-)Discrepancies", "-Discrepancies", "Minus Discrepancies") ?? 0,
                DeptDeduction: FindCol(map, "Deduction by department", "Dept Deduction", "Deduction by department (manager)") ?? 0,
                Delay: FindCol(map, "Delay") ?? 0
            );
        }

        private static int? FindCol(Dictionary<string, int> map, params string[] candidates)
        {
            foreach (var name in candidates)
            {
                if (map.TryGetValue(name, out int col)) return col;
                var match = map.Keys.FirstOrDefault(k =>
                    k.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                    name.Contains(k, StringComparison.OrdinalIgnoreCase));
                if (match != null) return map[match];
            }
            return null;
        }

        // ═══════════════════════════════════════════════════════════════
        //  PDF Generation — Single slip
        // ═══════════════════════════════════════════════════════════════

        private byte[] GeneratePdfSlip(ConfirmedSlipEntry entry)
        {
            var document = Document.Create(container =>
            {
                BuildPayslipPage(container, entry);
            });

            return document.GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════════════
        //  PDF Page Builder — shared between single & HR combined
        // ═══════════════════════════════════════════════════════════════

        private static void BuildPayslipPage(IDocumentContainer container, ConfirmedSlipEntry entry)
        {
            // ── Color palette ──
            var primaryColor = "#1a365d";      // Deep navy
            var accentColor = "#2b6cb0";       // Blue accent
            var successColor = "#276749";      // Green for net salary
            var successBg = "#f0fff4";         // Light green background
            var earningsBg = "#ebf8ff";        // Light blue for earnings
            var deductionsBg = "#fff5f5";      // Light red for deductions
            var lightGrey = "#f7fafc";         // Light background
            var borderColor = "#e2e8f0";       // Border grey
            var textPrimary = "#1a202c";       // Dark text
            var textSecondary = "#718096";      // Medium grey text

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(textPrimary));

                // ═══════════════════════════════════════
                //  HEADER
                // ═══════════════════════════════════════
                page.Header().Column(col =>
                {
                    // Company banner
                    col.Item().Background(primaryColor).Padding(20).Row(hr =>
                    {
                        hr.RelativeItem().Column(left =>
                        {
                            left.Item().Text("SALARY SLIP").Bold().FontSize(24).FontColor(Colors.White);
                            left.Item().PaddingTop(4).Text("HR Management System")
                                .FontSize(11).FontColor("#a0c4e8");
                        });

                        hr.ConstantItem(140).AlignRight().AlignMiddle().Column(right =>
                        {
                            right.Item().Text($"{DateTime.Now:MMMM yyyy}").Bold().FontSize(14).FontColor(Colors.White);
                            right.Item().PaddingTop(2).Text($"Date: {DateTime.Now:dd/MM/yyyy}")
                                .FontSize(10).FontColor("#a0c4e8");
                        });
                    });
                });

                // ═══════════════════════════════════════
                //  CONTENT
                // ═══════════════════════════════════════
                page.Content().PaddingVertical(15).Column(col =>
                {
                    // ─────────────────────────────────────
                    //  SECTION 1: Employee Information
                    // ─────────────────────────────────────
                    col.Item().PaddingBottom(5).Text("EMPLOYEE INFORMATION")
                        .Bold().FontSize(11).FontColor(accentColor);

                    col.Item().Border(1).BorderColor(borderColor).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.2f);  // Label
                            c.RelativeColumn(2);     // Value
                            c.RelativeColumn(1.2f);  // Label
                            c.RelativeColumn(2);     // Value
                        });

                        // Row 1: Name + Employee ID
                        InfoLabelCell(table, "Employee Name", lightGrey);
                        InfoValueCell(table, entry.ExcelName, lightGrey, true);
                        InfoLabelCell(table, "Employee ID", lightGrey);
                        InfoValueCell(table, entry.EmployeeId, lightGrey);

                        // Row 2: Department + Section
                        InfoLabelCell(table, "Department", lightGrey);
                        InfoValueCell(table, entry.Department, lightGrey);
                        InfoLabelCell(table, "Section", lightGrey);
                        InfoValueCell(table, entry.Section, lightGrey);

                        // Row 3: Job Title + Location
                        InfoLabelCell(table, "Job Title", lightGrey);
                        InfoValueCell(table, entry.JobTitle, lightGrey);
                        InfoLabelCell(table, "Location", lightGrey);
                        InfoValueCell(table, entry.Location, lightGrey);

                        // Row 4: Reporting To + Status
                        InfoLabelCell(table, "Reporting To", lightGrey);
                        InfoValueCell(table, entry.ReportingTo, lightGrey);
                        InfoLabelCell(table, "Status", lightGrey);
                        InfoValueCell(table, entry.Status, lightGrey);
                    });

                    col.Item().PaddingTop(18);

                    // ─────────────────────────────────────
                    //  SECTION 2: Gross Salary
                    // ─────────────────────────────────────
                    col.Item().Background(primaryColor).Padding(10).Row(r =>
                    {
                        r.RelativeItem().Text("GROSS SALARY").Bold().FontSize(12).FontColor(Colors.White);
                        r.ConstantItem(150).AlignRight().Text(FormatAmount(entry.GrossSalary))
                            .Bold().FontSize(14).FontColor(Colors.White);
                    });

                    col.Item().PaddingTop(15);

                    // Two columns: Earnings | Deductions
                    col.Item().Row(columns =>
                    {
                        // ─────────────────────────────────────
                        //  LEFT: Earnings / Allowances
                        // ─────────────────────────────────────
                        columns.RelativeItem().PaddingRight(8).Column(earningsCol =>
                        {
                            earningsCol.Item().Background(accentColor).Padding(8)
                                .Text("ALLOWANCES (+)").Bold().FontSize(10).FontColor(Colors.White);

                            earningsCol.Item().Border(1).BorderColor(borderColor).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(2);
                                });

                                // Table header
                                SalaryHeaderCell(table, "Description", earningsBg);
                                SalaryHeaderCell(table, "Amount", earningsBg);

                                // Allowance rows
                                SalaryRow(table, "Over Time", entry.OverTime);
                                SalaryRow(table, "Mission", entry.Mission, lightGrey);
                                SalaryRow(table, "(+) Discrepancies", entry.PlusDiscrepancies);
                                SalaryRow(table, "Bonus", entry.Bonus, lightGrey);

                                // Total Allowances
                                var totalAllowances = SumAmounts(entry.OverTime, entry.Mission, entry.PlusDiscrepancies, entry.Bonus);
                                table.Cell().BorderTop(2).BorderColor(accentColor)
                                    .Background(earningsBg).Padding(8)
                                    .Text("Total Allowances").Bold().FontSize(10).FontColor(accentColor);
                                table.Cell().BorderTop(2).BorderColor(accentColor)
                                    .Background(earningsBg).Padding(8).AlignRight()
                                    .Text(FormatAmount(totalAllowances)).Bold().FontSize(10).FontColor(accentColor);
                            });
                        });

                        // ─────────────────────────────────────
                        //  RIGHT: Deductions
                        // ─────────────────────────────────────
                        columns.RelativeItem().PaddingLeft(8).Column(deductionsCol =>
                        {
                            deductionsCol.Item().Background("#c53030").Padding(8)
                                .Text("DEDUCTIONS (−)").Bold().FontSize(10).FontColor(Colors.White);

                            deductionsCol.Item().Border(1).BorderColor(borderColor).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(2);
                                });

                                // Table header
                                SalaryHeaderCell(table, "Description", deductionsBg);
                                SalaryHeaderCell(table, "Amount", deductionsBg);

                                // Deduction rows
                                SalaryRow(table, "Social Security (5%)", entry.SS5);
                                SalaryRow(table, "(−) Discrepancies", entry.MinusDiscrepancies, lightGrey);
                                SalaryRow(table, "Dept. Deduction", entry.DeptDeduction);
                                SalaryRow(table, "Delay", entry.Delay, lightGrey);

                                // Total Deductions
                                var totalDeductions = SumAmounts(entry.SS5, entry.MinusDiscrepancies, entry.DeptDeduction, entry.Delay);
                                table.Cell().BorderTop(2).BorderColor("#c53030")
                                    .Background(deductionsBg).Padding(8)
                                    .Text("Total Deductions").Bold().FontSize(10).FontColor("#c53030");
                                table.Cell().BorderTop(2).BorderColor("#c53030")
                                    .Background(deductionsBg).Padding(8).AlignRight()
                                    .Text(FormatAmount(totalDeductions)).Bold().FontSize(10).FontColor("#c53030");
                            });
                        });
                    });

                    col.Item().PaddingTop(20);

                    // ─────────────────────────────────────
                    //  SECTION 3: NET SALARY — Final Result
                    // ─────────────────────────────────────
                    col.Item().Border(2).BorderColor(successColor).Background(successBg).Padding(16).Row(r =>
                    {
                        r.RelativeItem().AlignMiddle().Column(left =>
                        {
                            left.Item().Text("NET SALARY").Bold().FontSize(14).FontColor(successColor);
                            left.Item().PaddingTop(2).Text("Final amount after all allowances and deductions")
                                .FontSize(9).FontColor(textSecondary);
                        });

                        r.ConstantItem(180).AlignRight().AlignMiddle()
                            .Text(FormatAmount(entry.NetSalary))
                            .Bold().FontSize(22).FontColor(successColor);
                    });

                    col.Item().PaddingTop(25);

                    // ── Disclaimer ──
                    col.Item().PaddingTop(5)
                        .Text("This is a system-generated document and does not require a signature.")
                        .FontSize(9).FontColor(textSecondary).Italic();
                });

                // ═══════════════════════════════════════
                //  FOOTER
                // ═══════════════════════════════════════
                page.Footer().Background(lightGrey).BorderTop(1).BorderColor(borderColor).Padding(8).Row(r =>
                {
                    r.RelativeItem().AlignLeft().Text("HR Management System")
                        .FontSize(8).FontColor(textSecondary);
                    r.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(textSecondary);
                        x.CurrentPageNumber().FontSize(8).FontColor(textSecondary);
                        x.Span(" of ").FontSize(8).FontColor(textSecondary);
                        x.TotalPages().FontSize(8).FontColor(textSecondary);
                    });
                });
            });
        }

        // ═══════════════════════════════════════════════════════════════
        //  PDF Table Helper Methods
        // ═══════════════════════════════════════════════════════════════

        // Simplified helper approach
        private static void InfoLabelCell(TableDescriptor t, string label, string bg)
        {
            t.Cell().BorderBottom(1).BorderColor("#e2e8f0").Background(bg).Padding(8)
                .Text(label).SemiBold().FontSize(9).FontColor("#4a5568");
        }

        private static void InfoValueCell(TableDescriptor t, string value, string bg, bool bold = false)
        {
            var cell = t.Cell().BorderBottom(1).BorderColor("#e2e8f0").Background(bg).Padding(8);
            var text = cell.Text(string.IsNullOrWhiteSpace(value) ? "—" : value).FontSize(10);
            if (bold) text.Bold();
        }

        private static void SalaryHeaderCell(TableDescriptor t, string label, string bg)
        {
            t.Cell().Background(bg).Padding(8).Text(label).SemiBold().FontSize(9).FontColor("#4a5568");
        }

        private static void SalaryRow(TableDescriptor t, string label, string value, string? bg = null)
        {
            var bgColor = bg ?? Colors.White;
            t.Cell().BorderBottom(1).BorderColor("#edf2f7").Background(bgColor).Padding(8)
                .Text(label).FontSize(10);
            t.Cell().BorderBottom(1).BorderColor("#edf2f7").Background(bgColor).Padding(8).AlignRight()
                .Text(FormatAmount(value)).FontSize(10);
        }

        private static string FormatAmount(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "—";

            // Try to parse as number and format with commas
            if (decimal.TryParse(value.Replace(",", "").Trim(), out decimal num))
            {
                return num.ToString("N0");  // e.g., 1,500,000
            }
            return value.Trim();
        }

        private static string SumAmounts(params string[] amounts)
        {
            decimal total = 0;
            foreach (var a in amounts)
            {
                if (!string.IsNullOrWhiteSpace(a) && decimal.TryParse(a.Replace(",", "").Trim(), out decimal val))
                    total += val;
            }
            return total > 0 ? total.ToString() : "";
        }
    }
}

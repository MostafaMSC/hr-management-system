namespace HR.Application.Payroll.DTOs;

public class SalarySheetPreviewResponse
{
    public List<SalarySheetMatchDto> Matches { get; set; } = new();
    public List<MatchedUserDto> AllUsers { get; set; } = new();
    public int TotalRows { get; set; }
    public int MatchedRows { get; set; }
    public int UnmatchedRows { get; set; }
}

public class SalarySheetMatchDto
{
    public int RowIndex { get; set; }
    public string ExcelName { get; set; } = string.Empty;
    public MatchedUserDto? MatchedUser { get; set; }
    public string MatchType { get; set; } = "none";

    // Payroll fields
    public string? EmployeeId { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public string? JobTitle { get; set; }
    public string? ReportingTo { get; set; }
    public string? Department { get; set; }
    public string? Section { get; set; }
    public string? GrossSalary { get; set; }
    public string? NetSalary { get; set; }

    // Allowances
    public string? OverTime { get; set; }
    public string? Mission { get; set; }
    public string? PlusDiscrepancies { get; set; }
    public string? Bonus { get; set; }

    // Deductions
    public string? SS5 { get; set; }
    public string? MinusDiscrepancies { get; set; }
    public string? DeptDeduction { get; set; }
    public string? Delay { get; set; }
}

public class MatchedUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class SendSalarySlipsRequest
{
    public List<ConfirmedSlipEntry> Entries { get; set; } = new();
}

public class SendSalarySlipsResponse
{
    public int TotalProcessed { get; set; }
    public int EmailsSent { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Departments { get; set; } = new();
}

public class ConfirmedSlipEntry
{
    public int UserId { get; set; }
    public string ExcelName { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public string? JobTitle { get; set; }
    public string? ReportingTo { get; set; }
    public string? Department { get; set; }
    public string? Section { get; set; }
    public string? GrossSalary { get; set; }
    public string? NetSalary { get; set; }

    // Allowances
    public string? OverTime { get; set; }
    public string? Mission { get; set; }
    public string? PlusDiscrepancies { get; set; }
    public string? Bonus { get; set; }

    // Deductions
    public string? SS5 { get; set; }
    public string? MinusDiscrepancies { get; set; }
    public string? DeptDeduction { get; set; }
    public string? Delay { get; set; }
}

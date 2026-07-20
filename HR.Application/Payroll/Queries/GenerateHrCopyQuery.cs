using HR.Application.Common.Interfaces;
using HR.Application.Payroll.DTOs;
using MediatR;

namespace HR.Application.Payroll.Queries;

public record GenerateHrCopyQuery(SendSalarySlipsRequest Request, string? DepartmentFilter) : IRequest<byte[]>;

public class GenerateHrCopyQueryHandler : IRequestHandler<GenerateHrCopyQuery, byte[]>
{
    private readonly IPayrollService _payrollService;

    public GenerateHrCopyQueryHandler(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    public async Task<byte[]> Handle(GenerateHrCopyQuery request, CancellationToken cancellationToken)
    {
        return await _payrollService.GenerateHrCopyAsync(request.Request, request.DepartmentFilter, cancellationToken);
    }
}

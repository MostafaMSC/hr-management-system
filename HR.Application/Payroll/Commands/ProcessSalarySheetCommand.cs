using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Payroll.Commands;

public record ProcessSalarySheetCommand(Stream ExcelStream) : IRequest;

public class ProcessSalarySheetCommandHandler : IRequestHandler<ProcessSalarySheetCommand>
{
    private readonly IPayrollService _payrollService;

    public ProcessSalarySheetCommandHandler(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    public async Task Handle(ProcessSalarySheetCommand request, CancellationToken cancellationToken)
    {
        await _payrollService.ProcessSalarySheetAsync(request.ExcelStream, cancellationToken);
    }
}

using HR.Application.Common.Interfaces;
using HR.Application.Payroll.DTOs;
using MediatR;

namespace HR.Application.Payroll.Commands;

public record PreviewSalarySheetCommand(Stream ExcelStream) : IRequest<SalarySheetPreviewResponse>;

public class PreviewSalarySheetCommandHandler : IRequestHandler<PreviewSalarySheetCommand, SalarySheetPreviewResponse>
{
    private readonly IPayrollService _payrollService;

    public PreviewSalarySheetCommandHandler(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    public async Task<SalarySheetPreviewResponse> Handle(PreviewSalarySheetCommand request, CancellationToken cancellationToken)
    {
        return await _payrollService.PreviewSalarySheetAsync(request.ExcelStream, cancellationToken);
    }
}

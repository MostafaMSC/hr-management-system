using HR.Application.Common.Interfaces;
using HR.Application.Payroll.DTOs;
using MediatR;

namespace HR.Application.Payroll.Commands;

public record SendSalarySlipsCommand(SendSalarySlipsRequest Request) : IRequest<SendSalarySlipsResponse>;

public class SendSalarySlipsCommandHandler : IRequestHandler<SendSalarySlipsCommand, SendSalarySlipsResponse>
{
    private readonly IPayrollService _payrollService;

    public SendSalarySlipsCommandHandler(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    public async Task<SendSalarySlipsResponse> Handle(SendSalarySlipsCommand request, CancellationToken cancellationToken)
    {
        return await _payrollService.SendSalarySlipsAsync(request.Request, cancellationToken);
    }
}

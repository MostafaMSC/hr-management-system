using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Logs.Commands;



public class ResyncLogsCommandHandler : IRequestHandler<ResyncLogsCommand, ResyncLogsResult>
{
    public async Task<ResyncLogsResult> Handle(ResyncLogsCommand request, CancellationToken cancellationToken)
    {
        return new ResyncLogsResult { Success = true, Message = "" };
    }
}

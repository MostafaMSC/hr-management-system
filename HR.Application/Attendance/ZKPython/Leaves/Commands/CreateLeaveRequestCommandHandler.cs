using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Leaves.Commands
{
    public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, int>
    {
        private readonly ILeaveRepository _leaveRepository;

        public CreateLeaveRequestCommandHandler(ILeaveRepository leaveRepository)
        {
            _leaveRepository = leaveRepository;
        }

        public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            return 0;
        }
    }
}

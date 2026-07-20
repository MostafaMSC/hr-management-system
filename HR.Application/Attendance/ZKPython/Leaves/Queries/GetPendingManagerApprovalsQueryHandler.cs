using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Leaves.Queries
{
    public class GetPendingManagerApprovalsQueryHandler : IRequestHandler<GetPendingManagerApprovalsQuery, IEnumerable<LeaveRequestResponseDto>>
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IUserRepository _userRepository;

        public GetPendingManagerApprovalsQueryHandler(
            ILeaveRepository leaveRepository,
            IUserRepository userRepository)
        {
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<LeaveRequestResponseDto>> Handle(GetPendingManagerApprovalsQuery request, CancellationToken cancellationToken)
        {
        return new System.Collections.Generic.List<LeaveRequestResponseDto>();
    }
}
}

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
    public class GetPendingHRApprovalsQueryHandler : IRequestHandler<GetPendingHRApprovalsQuery, IEnumerable<LeaveRequestResponseDto>>
    {
        private readonly ILeaveRepository _leaveRepository;

        public GetPendingHRApprovalsQueryHandler(ILeaveRepository leaveRepository)
        {
            _leaveRepository = leaveRepository;
        }

        public async Task<IEnumerable<LeaveRequestResponseDto>> Handle(GetPendingHRApprovalsQuery request, CancellationToken cancellationToken)
        {
        return new System.Collections.Generic.List<LeaveRequestResponseDto>();
    }
}
}

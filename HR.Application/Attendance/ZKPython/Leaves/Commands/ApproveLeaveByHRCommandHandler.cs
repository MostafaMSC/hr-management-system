using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Leaves.Commands
{
    public class ApproveLeaveByHRCommandHandler : IRequestHandler<ApproveLeaveByHRCommand, bool>
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public ApproveLeaveByHRCommandHandler(
            ILeaveRepository leaveRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(ApproveLeaveByHRCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRepository.GetByIdAsync(request.LeaveRequestId);
            if (leaveRequest == null)
                throw new KeyNotFoundException($"Leave request with ID {request.LeaveRequestId} not found.");

            if (leaveRequest.Status != LeaveStatus.Pending)
                throw new InvalidOperationException("Leave request is not pending HR approval.");

            // Verify user has HR/Admin role
            var hrUser = await _userRepository.GetByIdAsync(request.HRUserId);
            if (hrUser == null)
                throw new KeyNotFoundException("HR user not found.");

            if (hrUser.Role != Domain.Enums.UserType.Administrator && hrUser.Role != Domain.Enums.UserType.HR)
                throw new UnauthorizedAccessException("Only HR/Admin users can approve leave requests at this stage.");

            // Update leave request
            leaveRequest.Status = LeaveStatus.Approved;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            await _leaveRepository.UpdateAsync(leaveRequest);

            // Send notification to employee
            await _notificationService.NotifyLeaveStatusChangedAsync(
                leaveRequest.UserInfoId,
                leaveRequest.Id,
                LeaveStatus.Approved,
                request.Comment);

            return true;
        }
    }
}

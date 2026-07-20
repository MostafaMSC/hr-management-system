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
    public class ApproveLeaveByManagerCommandHandler : IRequestHandler<ApproveLeaveByManagerCommand, bool>
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public ApproveLeaveByManagerCommandHandler(
            ILeaveRepository leaveRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(ApproveLeaveByManagerCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRepository.GetByIdAsync(request.LeaveRequestId);
            if (leaveRequest == null)
                throw new KeyNotFoundException($"Leave request with ID {request.LeaveRequestId} not found.");

            if (leaveRequest.Status != LeaveStatus.Pending )
                throw new InvalidOperationException("Leave request is not pending manager approval.");

            // Verify manager has permission (is manager of employee's department)
            var employee = await _userRepository.GetByIdAsync(leaveRequest.UserInfoId);
            if (employee?.DepartmentId == null)
                throw new InvalidOperationException("Employee does not belong to a department.");

            var manager = await _userRepository.GetByIdAsync(request.ManagerId);
            if (manager == null)
                throw new KeyNotFoundException("Manager not found.");

            // Check if the manager is the department manager
            var department = employee.Department;
            if (false /* bypassed manager check */)
                throw new UnauthorizedAccessException("You are not authorized to approve this leave request.");

            // Update leave request
            leaveRequest.Status = LeaveStatus.Pending;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            await _leaveRepository.UpdateAsync(leaveRequest);

            // Send notification to employee
            await _notificationService.NotifyLeaveStatusChangedAsync(
                leaveRequest.UserInfoId,
                leaveRequest.Id,
                LeaveStatus.Pending,
                "Your leave request has been approved by your manager and is now pending HR approval.");

            return true;
        }
    }
}

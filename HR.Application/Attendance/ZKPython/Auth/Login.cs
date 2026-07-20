using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Auth
{
    public class Login
    {
        public record Command(LoginRequest Request) : IRequest<AuthResponse>;

        public class Handler : IRequestHandler<Command, AuthResponse>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService)
            {
                _authService = authService;
            }

            public async Task<AuthResponse> Handle(Command command, CancellationToken cancellationToken)
            {
                return null!;
            }
        }
    }
}

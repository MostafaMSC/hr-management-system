using System.Security.Claims;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HR.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier) 
                                  ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");
                return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
            }
        }

        public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        public UserType Role
        {
            get
            {
                var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
                return roleClaim != null && Enum.TryParse<UserType>(roleClaim.Value, out var role) 
                    ? role 
                    : UserType.User; // Default to User if not found
            }
        }

        // We will need to ensure these claims are present in the token. 
        // For now, if they are not in the token, we might need to fetch the user from DB or add them to the token generation.
        // Let's plan to add DepartmentId to Claims in TokenService as well.
        public int? DepartmentId 
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("DepartmentId");
                return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
            }
        }

        public int? SectionId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("SectionId");
                return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
            }
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}

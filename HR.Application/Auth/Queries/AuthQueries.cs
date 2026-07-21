using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Auth.Queries;

// --- Queries ---
public record GetMeQuery(int UserId) : IRequest<object>;
public record GetUsersQuery(int Page = 1, int PageSize = 100, string? SearchTerm = null) : IRequest<object>;
public record Get2FAStatusQuery(int UserId) : IRequest<object>;
public record GetEmployeeStatsQuery(DateTime? Date) : IRequest<object>;

// --- Handlers ---
public class AuthQueriesHandler :
    IRequestHandler<GetMeQuery, object>,
    IRequestHandler<GetUsersQuery, object>,
    IRequestHandler<Get2FAStatusQuery, object>,
    IRequestHandler<GetEmployeeStatsQuery, object>
{
    private readonly IApplicationDbContext _context;

    public AuthQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos
            .Include(u => u.Department)
            .Include(u => u.Section)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) throw new KeyNotFoundException("User not found.");

        return new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role,
            departmentId = user.DepartmentId,
            departmentName = user.Department?.Name,
            sectionId = user.SectionId,
            sectionName = user.Section?.Name,
            photo = user.ProfilePictureUrl,
            phoneNumber = user.PhoneNumber,
            birthDate = user.BirthDate,
            hireDate = user.HireDate,
            gender = user.Gender,
            shiftType = user.ShiftType,
            address = user.Address,
            card = user.Card,
            accountStatus = user.AccountStatus
        };
    }

    public async Task<object> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserInfos
            .Include(u => u.Department)
            .Include(u => u.Section)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(u => 
                (u.Username != null && u.Username.ToLower().Contains(search)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search))
            );
        }

        var total = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new
        {
            success = true,
            total = total,
            page = request.Page,
            pageSize = request.PageSize,
            count = users.Count,
            data = users.Select(u => new
            {
                id = u.Id,
                username = u.Username,
                firstName = u.FirstName,
                lastName = u.LastName,
                email = u.Email,
                role = u.Role.ToString(),
                department = u.Department == null ? null : new { id = u.Department.Id, name = u.Department.Name },
                section = u.Section == null ? null : new { id = u.Section.Id, name = u.Section.Name },
                accountStatus = u.AccountStatus,
                phoneNumber = u.PhoneNumber,
                photo = u.ProfilePictureUrl,
                biometricId = u.BiometricId,
                hireDate = u.DateOfJoining,
                is2FAEnabled = u.Is2FAEnabled
            })
        };
    }

    public async Task<object> Handle(Get2FAStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found.");

        return new
        {
            twoFactorEnabled = user.Is2FAEnabled,
            twoFactorType = user.TwoFactorType
        };
    }

    public async Task<object> Handle(GetEmployeeStatsQuery request, CancellationToken cancellationToken)
    {
        // Dummy implementation to match legacy signature
        return await Task.FromResult(new { message = "Employee stats not fully implemented yet." });
    }
}

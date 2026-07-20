using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserInfo> UserInfos => Set<UserInfo>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<AttendanceShift> AttendanceShifts => Set<AttendanceShift>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<DailyAttendanceSummary> DailyAttendanceSummaries => Set<DailyAttendanceSummary>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<HR.Domain.Common.ISoftDelete>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply Global Query Filter for Soft Delete
        builder.Entity<UserInfo>().HasQueryFilter(u => !u.IsDeleted);

        // Configure UserInfo -> Department & Section
        builder.Entity<UserInfo>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<UserInfo>()
            .HasOne(u => u.Section)
            .WithMany(s => s.Employees)
            .HasForeignKey(u => u.SectionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Section>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Sections)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserDevice (Many-to-Many join table)
        builder.Entity<UserDevice>()
            .HasOne(ud => ud.UserInfo)
            .WithMany(u => u.UserDevices)
            .HasForeignKey(ud => ud.UserInfoId);

        builder.Entity<UserDevice>()
            .HasOne(ud => ud.Device)
            .WithMany(d => d.UserDevices)
            .HasForeignKey(ud => ud.DeviceId);
            
        // Configure RefreshTokens
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.UserInfo)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure Notifications
        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

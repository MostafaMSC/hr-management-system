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

    public DbSet<Fingerprint> Fingerprints => Set<Fingerprint>();

    public DbSet<BonusRequest> BonusRequests => Set<BonusRequest>();

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
        builder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Section>().HasQueryFilter(s => !s.IsDeleted);
        builder.Entity<AttendanceShift>().HasQueryFilter(s => !s.IsDeleted);

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

        // UserInfo Unique Constraints & Self-Referencing
        builder.Entity<UserInfo>().HasIndex(e => e.Username).IsUnique();
        
        builder.Entity<UserInfo>()
            .HasOne(u => u.DirectManager)
            .WithMany()
            .HasForeignKey(u => u.DirectManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserInfo>()
            .HasOne(u => u.ReportTo)
            .WithMany()
            .HasForeignKey(u => u.ReportToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserInfo>()
            .HasOne(u => u.SecondLineManager)
            .WithMany()
            .HasForeignKey(u => u.SecondLineManagerId)
            .OnDelete(DeleteBehavior.Restrict);

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
        builder.Entity<RefreshToken>().HasIndex(e => e.Token).IsUnique();
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

        // AttendanceLogs
        builder.Entity<AttendanceLog>()
            .HasIndex(a => new { a.UserInfoId, a.Time })
            .IsUnique();
        builder.Entity<AttendanceLog>()
            .HasIndex(a => a.DeviceIP);

        // LeaveBalances
        builder.Entity<LeaveBalance>()
            .HasIndex(lb => new { lb.UserInfoId, lb.Year })
            .IsUnique();

        // SystemSettings
        builder.Entity<SystemSetting>()
            .HasIndex(e => new { e.Section, e.Key })
            .IsUnique();
            
        // Holiday
        builder.Entity<Holiday>()
            .HasIndex(e => e.Date)
            .IsUnique();

        // DailyAttendanceSummary
        builder.Entity<DailyAttendanceSummary>()
            .HasIndex(das => new { das.UserInfoId, das.Date })
            .IsUnique();


        builder.Entity<BonusRequest>(entity =>
        {
            entity.HasOne(e => e.RequestingManager)
                  .WithMany(u => u.RequestedBonuses)
                  .HasForeignKey(e => e.RequestingManagerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetUser)
                  .WithMany(u => u.ReceivedBonuses)
                  .HasForeignKey(e => e.TargetUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProcessedByHr)
                  .WithMany(u => u.ProcessedBonuses)
                  .HasForeignKey(e => e.ProcessedByHrId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // LeaveRequest relationships
        builder.Entity<LeaveRequest>()
            .HasOne(lr => lr.UserInfo)
            .WithMany(u => u.LeaveRequests)
            .HasForeignKey(lr => lr.UserInfoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeaveRequest>()
            .HasOne(lr => lr.ApprovedByManager)
            .WithMany()
            .HasForeignKey(lr => lr.ApprovedByManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeaveRequest>()
            .HasOne(lr => lr.ApprovedBySecondLineManager)
            .WithMany()
            .HasForeignKey(lr => lr.ApprovedBySecondLineManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeaveRequest>()
            .HasOne(lr => lr.ApprovedByHR)
            .WithMany()
            .HasForeignKey(lr => lr.ApprovedByHRId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeaveRequest>()
            .HasOne(lr => lr.RejectedBy)
            .WithMany()
            .HasForeignKey(lr => lr.RejectedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Fingerprint
        builder.Entity<Fingerprint>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DeviceUserId);
            entity.HasIndex(e => e.DeviceIp);
            entity.HasIndex(e => new { e.DeviceUserId, e.FingerIndex, e.DeviceIp }).IsUnique();
            
            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed Data
        builder.Entity<SystemSetting>().HasData(
            new SystemSetting { Id = 1, Section = "Work", Key = "workDayStart", Value = "08:30", Description = "Default work start time" },
            new SystemSetting { Id = 2, Section = "Work", Key = "workDayEnd", Value = "16:00", Description = "Default work end time" },
            new SystemSetting { Id = 3, Section = "Work", Key = "requiredDailyHours", Value = "8", Description = "Required daily work hours" },
            new SystemSetting { Id = 4, Section = "Work", Key = "workingDaysPerMonth", Value = "26", Description = "Default working days per month" },
            new SystemSetting { Id = 5, Section = "Work", Key = "allowedMonthlyLeaveDays", Value = "1.7", Description = "Default allowed monthly leave days" },
            new SystemSetting { Id = 6, Section = "Work", Key = "allowedMonthlyLeaveHours", Value = "4", Description = "Default allowed monthly leave hours" },
            new SystemSetting { Id = 7, Section = "Work", Key = "allowedSickLeaveDays", Value = "15", Description = "Default allowed yearly sick leave days" },
            new SystemSetting { Id = 8, Section = "Work", Key = "firstDayOfWeek", Value = "6", Description = "First day of week (0=Sunday, 6=Saturday)" }
        );

        builder.Entity<AttendanceShift>().HasData(
            new AttendanceShift
            {
                Id = 1,
                Name = "Normal Shift",
                StartTime = new TimeSpan(8, 30, 0),
                EndTime = new TimeSpan(16, 0, 0)
            }
        );
    }
}

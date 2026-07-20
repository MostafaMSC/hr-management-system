using HR.Application.Common.Interfaces;
using HR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register Background Services
        services.AddHostedService<HR.Infrastructure.Services.LegacyDevicePollingService>();

        // Register other services here (ZKTecoService, EmailService)
        services.AddTransient<IReportExportService, HR.Infrastructure.Services.ReportExportService>();
        services.AddTransient<ITokenService, HR.Infrastructure.Services.JwtTokenService>();
        services.AddTransient<IPasswordHasher, HR.Infrastructure.Services.BCryptPasswordHasherService>();
        services.AddTransient<ILocalStorageService, HR.Infrastructure.Services.LocalFileStorageService>();
        services.AddSingleton<IFirebaseService, HR.Infrastructure.Services.FirebasePushNotificationService>();

        // Register the new FastAPI HTTP Client
        services.AddHttpClient<IAttendanceProvider, HR.Infrastructure.Services.ZKTecoHttpClientProvider>();

        // Register other device providers
        services.AddScoped<IAttendanceProvider, HR.Infrastructure.Services.ZkTecoTcpProvider>();
        services.AddScoped<IAttendanceProvider, HR.Infrastructure.Services.HikvisionHttpProvider>();

        // Register the Device Provider Factory
        services.AddScoped<IDeviceProviderFactory, HR.Infrastructure.Services.DeviceProviderFactory>();

        // Configure Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("accessToken"))
                    {
                        context.Token = context.Request.Cookies["accessToken"];
                    }
                    return Task.CompletedTask;
                }
            };
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Authorization Policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
                policy.RequireRole("Administrator"));

            options.AddPolicy("HRPolicy", policy =>
                policy.RequireRole("Administrator", "HR"));

            options.AddPolicy("ManagerPolicy", policy =>
                policy.RequireRole("Administrator", "Manager"));

            options.AddPolicy("EmployeePolicy", policy =>
                policy.RequireRole("Administrator", "Manager", "User", "HR"));
        });

        // Redis Caching
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            // abortConnect=false → don't crash if Redis is unavailable at startup
            options.Configuration = redisConnectionString + ",abortConnect=false";
            options.InstanceName = "FingerPrint_";
        });

        // Register IConnectionMultiplexer for prefix-based cache invalidation
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        {
            try
            {
                var config = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
                config.AbortOnConnectFail = false;
                config.ConnectTimeout = 3000;
                config.SyncTimeout = 3000;
                return StackExchange.Redis.ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<StackExchange.Redis.IConnectionMultiplexer>>();
                logger.LogWarning(ex, "⚠️ Redis is unavailable. Caching is disabled — app will use database directly.");
                return null!;
            }
        });

        services.AddScoped<IUserRepository, HR.Infrastructure.Repositories.UserRepository>();
        services.AddScoped<IAttendanceLogRepository, HR.Infrastructure.Repositories.AttendanceLogRepository>();
        services.AddScoped<IDepartmentRepository, HR.Infrastructure.Repositories.DepartmentRepository>();
        services.AddScoped<ISectionRepository, HR.Infrastructure.Repositories.SectionRepository>();
        services.AddScoped<ITicketRepository, HR.Infrastructure.Repositories.TicketRepository>();
        services.AddScoped<ILeaveRepository, HR.Infrastructure.Repositories.LeaveRepository>();
        services.AddScoped<ISettingsRepository, HR.Infrastructure.Repositories.SettingsRepository>();
        services.AddScoped<IRefreshTokenRepository, HR.Infrastructure.Repositories.RefreshTokenRepository>();
        services.AddScoped<IFingerprintRepository, HR.Infrastructure.Repositories.FingerprintRepository>();
        services.AddScoped<IDeviceRepository, HR.Infrastructure.Repositories.DeviceRepository>();
        services.AddScoped<INotificationRepository, HR.Infrastructure.Repositories.NotificationRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, HR.Infrastructure.Services.AuthService>();
        services.AddScoped<ICurrentUserService, HR.Infrastructure.Services.CurrentUserService>();
        services.AddScoped<IPythonService, HR.Infrastructure.Services.PythonService>();
        services.AddScoped<INotificationService, HR.Infrastructure.Services.NotificationService>();
        services.AddScoped<ICacheService, HR.Infrastructure.Services.RedisCacheService>();
        services.AddScoped<IAttendanceEvaluationService, HR.Infrastructure.Services.AttendanceEvaluationService>();
        services.AddScoped<IEmailService, HR.Infrastructure.Services.EmailService>();
        services.AddScoped<IPayrollService, HR.Infrastructure.Services.PayrollService>();

        return services;
    }
}

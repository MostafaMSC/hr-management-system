using HR.Application;
using HR.Infrastructure;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Exceptions;
using HR.WebApi.Middleware;
using HR.WebApi.Converters;
using HR.Infrastructure.Hubs;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Fallback configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("YOUR_DATABASE_CONNECTION_STRING_HERE"))
{
    connectionString = "Server=localhost;Database=HRManagementDB;User=root;Password=;";
    Console.WriteLine("⚠️ WARNING: Using Hardcoded Connection String Fallback!");
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// No longer needed for MySQL

// Add Services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new RawDateTimeConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ipKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 200,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10,
        });
    });
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddSignalR();
builder.Services.AddResponseCaching();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "_"));

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HR Management API",
        Version = "v1",
        Description = "An ASP.NET Core Web API for managing HR operations, attendance tracking, device synchronization, and user roles.",
        Contact = new OpenApiContact
        {
            Name = "HR System Support",
            Email = "support@hrsystem.local"
        }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    var appXmlFile = "HR.Application.xml";
    var appXmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, appXmlFile);
    if (System.IO.File.Exists(appXmlPath))
        c.IncludeXmlComments(appXmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>() as DbContext;
    try
    {
        Log.Information("🚀 Backend is starting, applying migrations...");
        dbContext?.Database.Migrate();
        Log.Information("✅ Migrations applied successfully.");

        // Device Seeding
        var deviceIpsFromConfig = app.Configuration.GetSection("Devices").Get<string[]>();
        var db = dbContext as HR.Infrastructure.Data.ApplicationDbContext;
        if (db != null)
        {
            if (deviceIpsFromConfig != null && deviceIpsFromConfig.Any())
            {
                var existingIps = db.Devices.AsNoTracking().Select(d => d.IpAddress).ToList();
                var addedCount = 0;
                foreach (var ip in deviceIpsFromConfig)
                {
                    if (!existingIps.Contains(ip))
                    {
                        Log.Information("🌱 Adding new device from config: {Ip}", ip);
                        db.Devices.Add(new Device
                        {
                            DeviceName = $"Device_{ip.Replace(".", "_")}",
                            IpAddress = ip
                        });
                        addedCount++;
                    }
                }
                if (addedCount > 0)
                {
                    db.SaveChanges();
                    Log.Information("✅ Successfully added {Count} new devices from configuration.", addedCount);
                }
            }

            // Seed Default Admin User
            var adminUser = db.UserInfos.FirstOrDefault(u => u.Email == "admin@admin.com");
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            if (adminUser == null)
            {
                Log.Information("🌱 Seeding default Admin user...");
                adminUser = new UserInfo
                {
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "admin@admin.com",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    Role = HR.Domain.Enums.UserType.Administrator,
                    BiometricId = "1", // Master biometric ID
                    AccountStatus = "Active",
                    DateOfJoining = DateTime.UtcNow
                };
                db.UserInfos.Add(adminUser);
            }
            else
            {
                // Force reset password to ensure it matches
                adminUser.PasswordHash = passwordHasher.HashPassword("Admin123!");
            }
            db.SaveChanges();
            Log.Information("✅ Default Admin user is ready. (Email: admin@admin.com, Pass: Admin123!)");
            
            // Mock Data Seeding for Testing
            Log.Information("🌱 Seeding comprehensive mock data...");
            await HR.Infrastructure.Data.Seeders.MockDataSeeder.SeedAsync(db, passwordHasher);
            Log.Information("✅ Comprehensive mock data seeded successfully.");
        }

        // Auto-Repair (If Holiday logic is needed, add here. Skipping explicit repair for now since Holiday entity is not standard in HR template yet)
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during startup seeding/migration.");
    }
}

// Middleware pipeline
app.UseGlobalExceptionHandler();
app.UseSerilogRequestLogging();
app.UseMiddleware<LogContextMiddleware>();

app.UseCors("DevCorsPolicy");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseResponseCaching();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RealTimeHub>("/realtimeHub");

app.MapFallbackToFile("index.html");

app.Run();

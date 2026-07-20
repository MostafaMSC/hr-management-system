using HR.Application.Attendance.DTOs;
using HR.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HR.Infrastructure.Services;

public class ZKTecoHttpClientProvider : IAttendanceProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZKTecoHttpClientProvider> _logger;

    public HR.Domain.Enums.DeviceProtocol SupportedProtocol => HR.Domain.Enums.DeviceProtocol.ZkTecoHttp;

    public ZKTecoHttpClientProvider(HttpClient httpClient, IConfiguration configuration, ILogger<ZKTecoHttpClientProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = configuration["PythonMicroservice:BaseUrl"];
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
        else
        {
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/");
        }
    }

    public async Task<List<AttendanceLogDto>> FetchLogsAsync(string deviceIp, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching logs from FastAPI for device {DeviceIp}", deviceIp);

            var response = await _httpClient.GetAsync($"/api/devices/{deviceIp}/logs", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var logs = await response.Content.ReadFromJsonAsync<List<AttendanceLogDto>>(cancellationToken: cancellationToken);
                return logs ?? new List<AttendanceLogDto>();
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("FastAPI returned error {StatusCode} for device {DeviceIp}: {Error}", response.StatusCode, deviceIp, errorMsg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Python FastAPI service for device {DeviceIp}", deviceIp);
        }

        return new List<AttendanceLogDto>(); // Return empty list on failure so the background worker doesn't crash
    }

    public async Task<bool> AddOrEditUserAsync(string deviceIp, AddEditDeviceUserDto user, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/devices/{deviceIp}/users", user, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to Add/Edit user on device {DeviceIp}", deviceIp);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string deviceIp, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/devices/{deviceIp}/users/{userId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to Delete user on device {DeviceIp}", deviceIp);
            return false;
        }
    }

    public async Task<bool> EnrollUserAsync(string deviceIp, EnrollRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting biometric enrollment on device {DeviceIp} for user {UserId}", deviceIp, request.UserId);

            // Adjust the timeout dynamically for interactive enrollment
            _httpClient.Timeout = TimeSpan.FromMinutes(3);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(2.5)); // Fallback cancel after 2.5 mins

            var response = await _httpClient.PostAsJsonAsync($"/api/devices/{deviceIp}/enroll", request, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete enrollment on device {DeviceIp}", deviceIp);
            return false;
        }
    }

    public async Task<bool> SyncTimeAsync(string deviceIp, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/devices/{deviceIp}/sync-time", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync time on device {DeviceIp}", deviceIp);
            return false;
        }
    }

    public async Task<bool> SyncFullUserAsync(string deviceIp, SyncUserRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/devices/{deviceIp}/sync-full-user", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync full user on device {DeviceIp}", deviceIp);
            return false;
        }
    }

    public async Task<List<DeviceUserResultDto>> FetchUsersAsync(string deviceIp, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/devices/{deviceIp}/users", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DeviceUserResultDto>>(cancellationToken: cancellationToken)
                       ?? new List<DeviceUserResultDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch users from device {DeviceIp}", deviceIp);
        }
        return new List<DeviceUserResultDto>();
    }
}

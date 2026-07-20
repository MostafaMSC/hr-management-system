using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces
{
    public class PythonResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorDetail { get; set; } = string.Empty;
        public JsonObject? Data { get; set; }
    }

    public interface IPythonService
    {
        Task<JsonObject> RunPythonAsync(string deviceIp, CancellationToken cancellationToken = default);
        Task<JsonObject> RunPythonAddUserAsync(string deviceIp, string userName, CancellationToken cancellationToken = default);
        Task<JsonObject> RunPythonEditUserAsync(string deviceIp, string userId, string newName, int? privilege = null, CancellationToken cancellationToken = default);
        Task<JsonObject> RunPythonDeleteUserAsync(string deviceIp, string userId, CancellationToken cancellationToken = default);
        Task<JsonObject> RunPythonGetUsersAsync(string deviceIp, CancellationToken cancellationToken = default);
        Task<JsonObject> RunPythonSyncTimeAsync(string deviceIp, CancellationToken cancellationToken = default);
        Task<JsonObject> RunPythonGetDeviceTimeAsync(string deviceIp, CancellationToken cancellationToken = default);
        Task<PythonResult> DeleteUserFromDeviceAsync(string deviceIp, string deviceUserId);
        Task<PythonResult> AddOrEditUserOnDeviceAsync(string deviceIp, UserInfo user);
        Task<PythonResult> SyncFingerprintsToDeviceAsync(string deviceIp, string deviceUserId, List<Fingerprint> fingerprints);
        Task<PythonResult> SyncFullUserAsync(string deviceIp, UserInfo user);
        Task<JsonObject> RunPythonGetTemplatesAsync(string deviceIp, CancellationToken cancellationToken = default);
        Task<PythonResult> RunPythonEnrollUserAsync(string deviceIp, string userId, int tempId = 0, CancellationToken cancellationToken = default);
    }
}

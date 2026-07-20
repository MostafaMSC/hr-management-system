using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;

namespace HR.Infrastructure.Services
{
    public class PythonService : IPythonService
    {
        public Task<JsonObject> RunPythonAsync(string deviceIp, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<JsonObject> RunPythonAddUserAsync(string deviceIp, string userName, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<JsonObject> RunPythonEditUserAsync(string deviceIp, string userId, string newName, int? privilege = null, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<JsonObject> RunPythonDeleteUserAsync(string deviceIp, string userId, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<JsonObject> RunPythonGetUsersAsync(string deviceIp, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<JsonObject> RunPythonSyncTimeAsync(string deviceIp, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<JsonObject> RunPythonGetDeviceTimeAsync(string deviceIp, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<PythonResult> DeleteUserFromDeviceAsync(string deviceIp, string deviceUserId) => Task.FromResult(new PythonResult());
        public Task<PythonResult> AddOrEditUserOnDeviceAsync(string deviceIp, UserInfo user) => Task.FromResult(new PythonResult());
        public Task<PythonResult> SyncFingerprintsToDeviceAsync(string deviceIp, string deviceUserId, List<Fingerprint> fingerprints) => Task.FromResult(new PythonResult());
        public Task<PythonResult> SyncFullUserAsync(string deviceIp, UserInfo user) => Task.FromResult(new PythonResult());
        public Task<JsonObject> RunPythonGetTemplatesAsync(string deviceIp, CancellationToken cancellationToken = default) => Task.FromResult(new JsonObject());
        public Task<PythonResult> RunPythonEnrollUserAsync(string deviceIp, string userId, int tempId = 0, CancellationToken cancellationToken = default) => Task.FromResult(new PythonResult());
    }
}

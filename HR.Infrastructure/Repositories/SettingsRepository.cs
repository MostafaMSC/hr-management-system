using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
namespace HR.Infrastructure.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        public Task<Dictionary<string, string>> GetSettingsBySectionAsync(string section, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
        public Task UpdateSettingsAsync(string section, Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<string?> GetSettingAsync(string section, string key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }
}

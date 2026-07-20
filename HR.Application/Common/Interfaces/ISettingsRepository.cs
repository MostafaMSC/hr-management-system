using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.Application.Common.Interfaces
{
    public interface ISettingsRepository
    {
        Task<Dictionary<string, string>> GetSettingsBySectionAsync(string section, CancellationToken cancellationToken = default);
        Task UpdateSettingsAsync(string section, Dictionary<string, string> settings, CancellationToken cancellationToken = default);
        Task<string?> GetSettingAsync(string section, string key, CancellationToken cancellationToken = default);
    }
}

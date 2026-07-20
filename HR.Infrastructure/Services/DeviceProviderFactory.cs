using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Infrastructure.Services;

public class DeviceProviderFactory : IDeviceProviderFactory
{
    private readonly IEnumerable<IAttendanceProvider> _providers;

    public DeviceProviderFactory(IEnumerable<IAttendanceProvider> providers)
    {
        _providers = providers;
    }

    public IAttendanceProvider GetProvider(DeviceProtocol protocol)
    {
        var provider = _providers.FirstOrDefault(p => p.SupportedProtocol == protocol);
        if (provider == null)
        {
            throw new NotSupportedException($"No provider implemented for protocol: {protocol}");
        }
        return provider;
    }
}

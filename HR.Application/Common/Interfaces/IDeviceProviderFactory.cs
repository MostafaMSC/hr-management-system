using HR.Domain.Enums;

namespace HR.Application.Common.Interfaces;

public interface IDeviceProviderFactory
{
    IAttendanceProvider GetProvider(DeviceProtocol protocol);
}

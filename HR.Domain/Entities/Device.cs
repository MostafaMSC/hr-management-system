using HR.Domain.Common;

namespace HR.Domain.Entities;

public class Device : BaseEntity
{
    public string DeviceName { get; set; } = string.Empty;
    public string Name { get => DeviceName; set => DeviceName = value; }
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 4370;
    public string? SerialNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public HR.Domain.Enums.DeviceProtocol Protocol { get; set; } = HR.Domain.Enums.DeviceProtocol.ZkTecoTcp;

    // Navigation
    public ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
}

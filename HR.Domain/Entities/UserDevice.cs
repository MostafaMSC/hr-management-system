using HR.Domain.Common;

namespace HR.Domain.Entities;

public class UserDevice : BaseEntity
{
    public int UserInfoId { get; set; }
    public UserInfo UserInfo { get; set; } = null!;

    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string? ZkEnrollNumber { get; set; }
}

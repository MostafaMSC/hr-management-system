using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class AttendanceLog : BaseEntity
{
    public int UserInfoId { get; set; }
    public UserInfo UserInfo { get; set; } = null!;

    public int UserID { get => UserInfoId; set => UserInfoId = value; }
    public string Name { get => UserInfo != null ? $"{UserInfo.FirstName} {UserInfo.LastName}" : string.Empty; }
    public string CheckStatus { get => PunchType.ToString(); set { } }

    public int? DeviceId { get; set; }
    public Device? Device { get; set; }

    public DateTime PunchTime { get; set; }
    public DateTime Time { get => PunchTime; set => PunchTime = value; }
    public LogType Type { get => LogsType; set => LogsType = value; }

    public LogType LogsType { get; set; } = LogType.FingerPrint;
    public PunchType PunchType { get; set; }
}

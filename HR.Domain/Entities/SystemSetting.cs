using HR.Domain.Common;

namespace HR.Domain.Entities;

public class SystemSetting : BaseEntity
{
    public string Section { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

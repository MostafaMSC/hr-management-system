using HR.Domain.Common;

namespace HR.Domain.Entities;

public class Holiday : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}

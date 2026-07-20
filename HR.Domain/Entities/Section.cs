using HR.Domain.Common;

namespace HR.Domain.Entities;

public class Section : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    // Navigation properties
    public ICollection<UserInfo> Employees { get; set; } = new List<UserInfo>();
}

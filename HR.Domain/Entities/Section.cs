using HR.Domain.Common;

namespace HR.Domain.Entities;

public class Section : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    // Navigation properties
    public ICollection<UserInfo> Employees { get; set; } = new List<UserInfo>();

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

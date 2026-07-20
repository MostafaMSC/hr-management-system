using HR.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
    
    [ForeignKey("ManagerId")]
    public UserInfo? Manager { get; set; }

    // Navigation properties
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<UserInfo> Employees { get; set; } = new List<UserInfo>();
}

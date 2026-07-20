namespace HR.Domain.Entities
{
    public class Asset : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public UserInfo? AssignedToUser { get; set; }
    }

}

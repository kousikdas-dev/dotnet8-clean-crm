namespace Crm.Application.DTOs.DynamicEntities
{
    public class DynamicEntityDto
    {
        public Guid Id { get; set; }
        public string TenantId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string? SchemaJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ✅ Add this line
        public ICollection<DynamicFieldDto> Fields { get; set; } = new List<DynamicFieldDto>();
    }
}

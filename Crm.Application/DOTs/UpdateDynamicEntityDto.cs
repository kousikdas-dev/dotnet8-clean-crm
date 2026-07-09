namespace Crm.Application.DTOs.DynamicEntities
{
    public class UpdateDynamicEntityDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string? SchemaJson { get; set; }
        public ICollection<DynamicFieldDto> Fields { get; set; } = new List<DynamicFieldDto>();
    }
}

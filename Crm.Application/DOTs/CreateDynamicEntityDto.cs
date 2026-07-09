using Crm.Application.DTOs.DynamicUi;

namespace Crm.Application.DTOs.DynamicEntities
{
    public class CreateDynamicEntityDto
    {
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }
        public string? SchemaJson { get; set; }

        public string? Category { get; set; }
        public string? PrimaryField { get; set; }
        public string Icon { get; set; } = "box";
        public string Color { get; set; } = "#3b82f6"; // default blue
        
        public ICollection<DynamicFieldDto> Fields { get; set; } = new List<DynamicFieldDto>();

        // ✅ REQUIRED for dynamic UI
        public ICollection<DynamicRelationshipDto> RelationshipsFrom { get; set; } = new List<DynamicRelationshipDto>();
    }
}

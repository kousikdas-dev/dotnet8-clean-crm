namespace Crm.Application.DTOs.DynamicEntities
{
    public class UpdateDynamicFieldDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string DataType { get; set; } = null!;
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
    }
}

namespace Crm.Domain.Entities.Dynamic
{
    public class DynamicFieldOption : BaseEntity //Dropdown / Choice Options
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FieldId { get; set; }
        public DynamicField Field { get; set; } = null!;
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
    }
}

namespace Crm.Domain.Entities.Dynamic
{
    public class DynamicView : BaseEntity //UI Definition (Form + Grid Layout)
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EntityId { get; set; }
        public DynamicEntity Entity { get; set; } = null!;
        public string ViewType { get; set; } = "form"; // form, list, kanban, etc.
        public string Name { get; set; } = string.Empty;
        public string? LayoutJson { get; set; } // store JSON for layout structure
        public bool IsDefault { get; set; } = false;
    }
}

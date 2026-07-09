namespace Crm.Domain.Entities.Dynamic
{
    public class DynamicRelationship : BaseEntity // Entity Relationships //(like Odoo’s “One2Many”, “Many2One”).
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SourceEntityId { get; set; }
        public DynamicEntity SourceEntity { get; set; } = null!;

        public Guid TargetEntityId { get; set; }
        public DynamicEntity TargetEntity { get; set; } = null!;

        public string RelationType { get; set; } = "OneToMany"; // OneToOne, OneToMany, ManyToMany
        public string? SourceFieldName { get; set; } // e.g. "CustomerId"
        public string? TargetFieldName { get; set; } // e.g. "Orders"

        public bool CascadeDelete { get; set; } = false;
    }
}

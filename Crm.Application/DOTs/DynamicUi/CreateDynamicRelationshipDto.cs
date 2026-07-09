using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicUi
{
    public class CreateDynamicRelationshipDto
    {
        [Required] public Guid SourceEntityId { get; set; }
        [Required] public Guid TargetEntityId { get; set; }
        [Required] public string RelationType { get; set; } = "OneToMany"; // or "ManyToOne", etc.
        public string? SourceFieldName { get; set; }
        public string? TargetFieldName { get; set; }
        public bool CascadeDelete { get; set; } = false;
    }

    public class UpdateDynamicRelationshipDto : CreateDynamicRelationshipDto
    {
        [Required] public Guid Id { get; set; }
    }

    public class DynamicRelationshipDto : UpdateDynamicRelationshipDto
    {
        public bool IsDeleted { get; set; }
    }
}

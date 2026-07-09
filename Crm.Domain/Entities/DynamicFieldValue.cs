using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Entities
{
    [Table("DynamicFieldValues")]
    public class DynamicFieldValue : BaseEntity
    {
        [Required]
        public Guid RecordId { get; set; }

        [ForeignKey(nameof(RecordId))]
        public DynamicRecord Record { get; set; } = default!;

        [Required]
        public Guid FieldId { get; set; }

        [ForeignKey(nameof(FieldId))]
        public DynamicField? Field { get; set; } = default!;

        [MaxLength(200)]
        public string FieldName { get; set; } = string.Empty;

        public string? Value { get; set; } // Stored as string (convert in service)
        


    }
}

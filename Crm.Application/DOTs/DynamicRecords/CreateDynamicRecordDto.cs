using Crm.Application.DTOs.DynamicFieldValues;
using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicRecords
{
    public class CreateDynamicRecordDto
    {
        [Required]
        public Guid EntityId { get; set; }

        [MaxLength(100)]
        public string? RecordKey { get; set; }

        [Required]
        public string DataJson { get; set; } = "{}";

        [MaxLength(300)]
        public string? Summary { get; set; }

        [MaxLength(100)]
        public string? PrimaryDisplayField { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } = "Active";

        // ✅ Nested field values
        public List<CreateDynamicFieldValueDto> FieldValues { get; set; } = new();
    }    
}

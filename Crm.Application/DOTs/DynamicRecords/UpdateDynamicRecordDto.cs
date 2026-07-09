using Crm.Application.DTOs.DynamicFieldValues;
using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicRecords
{
    public class UpdateDynamicRecordDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [MaxLength(100)]
        public string RecordKey { get; set; } = string.Empty;

        [Required]
        public string DataJson { get; set; } = "{}";

        [MaxLength(300)]
        public string? Summary { get; set; }

        [MaxLength(100)]
        public string? PrimaryDisplayField { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } = "Active";

        // ✅ Add this list — it represents each field/value pair for the record
        public List<UpdateDynamicFieldValueDto> FieldValues { get; set; } = new();
    }
    
}

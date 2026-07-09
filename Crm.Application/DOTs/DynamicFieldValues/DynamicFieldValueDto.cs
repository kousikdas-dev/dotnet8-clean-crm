using System;

namespace Crm.Application.DTOs.DynamicFieldValues
{
    public class DynamicFieldValueDto
    {
        public Guid Id { get; set; }

        public Guid RecordId { get; set; }
        public Guid FieldId { get; set; }

        public string FieldName { get; set; } = string.Empty;

        public string? Value { get; set; }

        // Optional display or relational info
        public string? RecordKey { get; set; } // from DynamicRecord
        public string? EntityName { get; set; } // from related Entity

        public string? FieldLabel { get; set; }
        public Guid? EntityId { get; set; }
    }
}

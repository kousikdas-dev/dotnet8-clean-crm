using System;

namespace Crm.Application.DTOs.DynamicRecords
{
    public class DynamicRecordDto
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string RecordKey { get; set; } = string.Empty;
        public string DataJson { get; set; } = "{}";
        public DateTime CreatedAt { get; set; }
        public string? Summary { get; set; }
        public string? PrimaryDisplayField { get; set; }
        public string? Status { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // 👇 Optional convenience property (display name for related entity)
        public string? EntityName { get; set; }
    }
}

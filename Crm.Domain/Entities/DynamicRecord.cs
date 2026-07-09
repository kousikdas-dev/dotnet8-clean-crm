using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Entities
{
    [Table("DynamicRecords")]
    public class DynamicRecord : BaseEntity
    {
        // 🔗 Parent entity type (e.g., “Customer”, “Invoice”)
        [Required]
        public Guid EntityId { get; set; }

        [ForeignKey(nameof(EntityId))]
        public DynamicEntity Entity { get; set; } = default!;

        // 🧾 Record identifier (used as "primary key" for UI level)
        [MaxLength(100)]
        public string RecordKey { get; set; } = Guid.NewGuid().ToString();

        // 🧩 Actual dynamic data stored as JSON
        [Required]
        public string DataJson { get; set; } = "{}";

        // 🧠 Optional: denormalized for search/sorting
        [MaxLength(300)]
        public string? Summary { get; set; } // e.g. "John Doe - Active"

        [MaxLength(100)]
        public string? PrimaryDisplayField { get; set; } // e.g., “Name”

        // 📦 STATE MANAGEMENT ------------------------------------
        //public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? Status { get; set; } = "Active";

        

        // 🧩 RELATIONSHIP to FIELD VALUES (if you want fine-grained storage)
        public ICollection<DynamicFieldValue>? FieldValues { get; set; }
    }
}

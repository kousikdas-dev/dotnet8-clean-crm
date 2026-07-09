using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Domain.Entities
{
    public abstract class BaseEntity
    {
        // 🆔 Primary Key
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // 🏢 Multi-Tenancy
        [Required]
        [MaxLength(100)]
        public string TenantId { get; set; } = string.Empty;

        // 🕒 Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(200)]
        public string? CreatedByUserId { get; set; }

        [MaxLength(200)]
        public string? UpdatedByUserId { get; set; }

        // 🗑️ Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [MaxLength(200)]
        public string? DeletedByUserId { get; set; }

        // ✅ Optional: helper property to check active records
        public bool IsActive => !IsDeleted;
    }
}

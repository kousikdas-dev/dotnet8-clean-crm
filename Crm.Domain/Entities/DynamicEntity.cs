using Crm.Domain.Entities.Dynamic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Entities
{
    [Table("DynamicEntities")]
    public class DynamicEntity : BaseEntity
    {
        // 🏷️ BASIC DETAILS -------------------------------
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string SchemaJson { get; set; } = "[]";

        [MaxLength(100)]
        public string Category { get; set; } = "General";

        // 🧩 STRUCTURE SETTINGS --------------------------
        [MaxLength(100)]
        public string TableName { get; set; } = string.Empty;

        public bool IsSystem { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public bool EnableAuditTrail { get; set; } = true;
        public bool EnableSoftDelete { get; set; } = true;

        // 🔗 HIERARCHY -----------------------------------
        public Guid? ParentEntityId { get; set; }
        [ForeignKey(nameof(ParentEntityId))]
        public DynamicEntity? ParentEntity { get; set; }

        public ICollection<DynamicEntity>? ChildEntities { get; set; }

        // 🧠 UI / BEHAVIOR METADATA ----------------------
        [MaxLength(100)]
        public string PrimaryField { get; set; } = "Name";

        [MaxLength(50)]
        public string Icon { get; set; } = "database";

        [MaxLength(50)]
        public string Color { get; set; } = "#0078D4";

        [MaxLength(100)]
        public string DefaultSortField { get; set; } = "CreatedAt";

        [MaxLength(10)]
        public string DefaultSortOrder { get; set; } = "DESC";

        // ⚙️ AUTOMATION HOOKS ----------------------------
        public string? OnCreateScript { get; set; }
        public string? OnUpdateScript { get; set; }
        public string? OnDeleteScript { get; set; }

        // 👥 SECURITY / PERMISSIONS -----------------------
        public bool AllowPublicAccess { get; set; } = false;
        public bool AllowRecordSharing { get; set; } = true;
        public bool AllowOwnership { get; set; } = true;

        [MaxLength(200)]
        public string RolesAllowed { get; set; } = "";

        // 🧾 FORM / LAYOUT SETTINGS ----------------------
        public string? FormLayoutJson { get; set; }
        public string? ListViewJson { get; set; }

        // 🔗 FIELDS & RECORDS -----------------------------
        public virtual ICollection<DynamicField> Fields { get; set; } = new List<DynamicField>();
        public virtual ICollection<DynamicRecord> Records { get; set; } = new List<DynamicRecord>();


        // ✅✅ ADD THESE (Missing Relationship Navigation)
        // ------------------------------------------------------------------------------
        public virtual ICollection<DynamicRelationship> RelationshipsFrom { get; set; }
            = new List<DynamicRelationship>();

        public virtual ICollection<DynamicRelationship> RelationshipsTo { get; set; }
            = new List<DynamicRelationship>();

        // ✅ Permissions
        public virtual ICollection<DynamicPermission> Permissions { get; set; }
            = new List<DynamicPermission>();

        // ✅ Views
        public virtual ICollection<DynamicView> Views { get; set; }
            = new List<DynamicView>();

        // ✅ Field options are linked through DynamicField, so no need to add here
    }
}

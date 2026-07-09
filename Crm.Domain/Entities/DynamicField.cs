using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Entities
{
    [Table("DynamicFields")]
    public class DynamicField : BaseEntity
    {
        // 🔗 Relationship to parent entity
        [Required]
        public Guid EntityId { get; set; }

        [ForeignKey(nameof(EntityId))]
        public virtual DynamicEntity Entity { get; set; } = default!;

        // 🏷️ BASIC PROPERTIES --------------------------------
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Email", "Status"

        // 🧾 Display Label (used in UI)
        [Required]
        [MaxLength(150)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty; // e.g., "Customer Email"

        [MaxLength(100)]
        public string FieldType { get; set; } = "Text"; // Text, Number, Date, Boolean, Lookup, Dropdown, MultiSelect, Formula, File, Image, etc.

        [MaxLength(50)]
        public string DataType { get; set; } = "string"; // string, int, decimal, bool, datetime, json, etc.

        [MaxLength(50)]
        public string ControlType { get; set; } = "textbox"; // textbox, textarea, select, checkbox, datepicker, lookup, etc.

        [MaxLength(100)]
        public string GroupName { get; set; } = "General"; // For form grouping or sectioning

        // ⚙️ BEHAVIOR FLAGS ----------------------------------
        public bool IsRequired { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public bool IsSearchable { get; set; } = true;
        public bool IsReadOnly { get; set; } = false;
        public bool IsVisibleInList { get; set; } = true;
        public bool IsFilterable { get; set; } = true;

        // 👁 Whether field is visible in forms/lists
        public bool IsVisible { get; set; } = true;

        // 🎨 UI PROPERTIES ------------------------------------
        [MaxLength(50)]
        public string Placeholder { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Icon { get; set; } = "edit";

        public int DisplayOrder { get; set; } = 0;

        [MaxLength(20)]
        public string Align { get; set; } = "left"; // For grids

        public int ColumnWidth { get; set; } = 150; // For list/table display

        // 🔗 LOOKUP / RELATIONSHIP ----------------------------
        public Guid? LookupEntityId { get; set; } // For lookup fields referencing another entity
        [ForeignKey(nameof(LookupEntityId))]
        public DynamicEntity? LookupEntity { get; set; }

        [MaxLength(100)]
        public string LookupField { get; set; } = "Name"; // Display field from lookup entity

        // 🧠 DROPDOWN / MULTISELECT OPTIONS -------------------
        public string? OptionsJson { get; set; } // JSON like ["New","In Progress","Closed"]

        // 🔢 DEFAULTS & FORMULAS ------------------------------
        public string? DefaultValue { get; set; } // e.g., "Active" or "Today()"
        public string? Formula { get; set; } // e.g., "Quantity * Price"

        // 🧪 VALIDATION RULES ---------------------------------
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        // 🔹 Numeric fields (optional)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxValue { get; set; }

        [MaxLength(500)]
        public string? RegexPattern { get; set; } // For custom validation

        [MaxLength(200)]
        public string? ValidationMessage { get; set; } // For UI error messages

        // 🧾 CONDITIONAL LOGIC --------------------------------
        public string? VisibilityRule { get; set; } // e.g., "Status == 'Active'"
        public string? ReadOnlyRule { get; set; }
        public string? RequiredRule { get; set; }

        // ⚙️ ADVANCED OPTIONS --------------------------------
        public bool EnableAuditTrail { get; set; } = true;
        public bool EnableChangeTracking { get; set; } = false;

        // 🧍 OWNERSHIP & SECURITY ------------------------------
        public bool RestrictByRole { get; set; } = false;

        [MaxLength(200)]
        public string? RolesAllowed { get; set; } // e.g., "Admin,SalesManager"

        // 📄 HELP / TOOLTIP -----------------------------------
        [MaxLength(300)]
        public string? HelpText { get; set; }

        // 📊 LAYOUT SETTINGS ----------------------------------
        public string? StyleJson { get; set; } // e.g., {"color":"#000","fontWeight":"bold"}

        // 🔢 RELATIONSHIP ORDER -------------------------------
        public int Order { get; set; } = 0;
        // ✅ Add this line
        public ICollection<DynamicFieldValue> FieldValues { get; set; } = new List<DynamicFieldValue>();
    }
}

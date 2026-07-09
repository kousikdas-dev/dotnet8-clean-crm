using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Entities
{
    [Table("LeadAttachments")]
    public class LeadAttachment : BaseEntity
    {
        [Required]
        public Guid LeadRecordId { get; set; }

        [MaxLength(260)]
        public string OriginalFileName { get; set; } = string.Empty;

        [MaxLength(128)]
        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        [MaxLength(512)]
        public string StoragePath { get; set; } = string.Empty; // relative path under wwwroot
    }
}


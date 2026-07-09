namespace Crm.Domain.Entities
{
    public class Contact : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Company { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public string CreatedByUserId { get; set; } = null!;

        // ✅ Add these fields for update tracking
        //public DateTime? UpdatedAt { get; set; }
        //public string? UpdatedByUserId { get; set; }

        // Read-only property for convenience (do not assign to this)
        public string FullName => $"{FirstName} {LastName}";
    }
}

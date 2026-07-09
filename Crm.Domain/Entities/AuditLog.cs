namespace Crm.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string TenantId { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public Guid EntityId { get; set; }

        public string? PropertyName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public string? ActionType { get; set; } // Create, Update, Delete
        public string? UserId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

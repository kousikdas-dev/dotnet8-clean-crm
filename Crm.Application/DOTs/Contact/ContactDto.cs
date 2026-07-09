using System;

namespace Crm.Application.DOTs.Contact
{
    public class ContactDto
    {
        public Guid Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Company { get; set; }

        public string? TenantId { get; set; }

        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

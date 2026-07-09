using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DOTs.Contact
{
    public class CreateContactDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Company { get; set; }
    }
}

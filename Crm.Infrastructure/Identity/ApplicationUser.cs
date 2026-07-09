using Microsoft.AspNetCore.Identity;
using System;

namespace Crm.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string? RoleType { get; set; } // e.g., Admin, Manager, User

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 👇 Add these two lines for Refresh Token support
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}

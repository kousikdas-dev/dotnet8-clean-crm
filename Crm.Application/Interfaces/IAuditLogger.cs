using Crm.Domain.Entities;

namespace Crm.Application.Interfaces
{
    public interface IAuditLogger
    {
        Task LogAsync(AuditLog log);
    }
}

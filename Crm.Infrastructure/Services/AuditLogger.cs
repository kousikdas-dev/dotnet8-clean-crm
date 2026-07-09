using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;

namespace Crm.Infrastructure.Services
{
    public class AuditLogger : IAuditLogger
    {
        private readonly ApplicationDbContext _context;

        public AuditLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}

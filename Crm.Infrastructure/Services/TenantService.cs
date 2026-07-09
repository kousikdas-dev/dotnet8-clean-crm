using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant> CreateTenantAsync(string name)
        {
            var tenant = new Tenant
            {
                Name = name
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return tenant;
        }
    }

}

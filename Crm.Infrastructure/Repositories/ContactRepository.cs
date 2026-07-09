using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public ContactRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();

        public async Task<IEnumerable<Contact>> GetAllAsync()
        {
            // ✅ Optimized with AsNoTracking for better performance on read-only queries
            return await _context.Contacts
                .Where(c => c.TenantId == TenantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Contact?> GetByIdAsync(Guid id)
        {
            return await _context.Contacts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == TenantId);
        }

        public async Task<Contact> CreateAsync(Contact contact)
        {
            // ✅ Automatically set tenant and user
            contact.TenantId = _tenantProvider.GetTenantId();
            contact.CreatedByUserId = _tenantProvider.GetUserId();
            contact.CreatedAt = DateTime.UtcNow;

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<Contact> UpdateAsync(Contact contact)
        {
            // ✅ Find existing contact for the same tenant
            var existing = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == contact.Id && c.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Contact does not belong to current tenant.");

            // ✅ Update only allowed fields
            existing.FirstName = contact.FirstName;
            existing.LastName = contact.LastName;
            existing.Email = contact.Email;
            existing.Phone = contact.Phone;
            existing.Company = contact.Company;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            _context.Contacts.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == TenantId);

            if (contact == null)
                return false;

            // ✅ Perform soft delete instead of actual removal
            contact.IsDeleted = true;
            contact.DeletedAt = DateTime.UtcNow;
            contact.DeletedByUserId = _tenantProvider.GetUserId();

            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            var contact = await _context.Contacts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == TenantId);

            if (contact == null || !contact.IsDeleted)
                return false;

            contact.IsDeleted = false;
            contact.DeletedAt = null;
            contact.DeletedByUserId = null;

            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}

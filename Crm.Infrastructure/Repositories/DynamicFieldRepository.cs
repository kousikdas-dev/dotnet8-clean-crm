using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicFieldRepository : IDynamicFieldRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicFieldRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all dynamic fields for tenant
        public async Task<IEnumerable<DynamicField>> GetAllAsync()
        {
            return await _context.DynamicFields
                // Allow "global" fields where TenantId is empty
                .Where(f => (f.TenantId == TenantId || f.TenantId == string.Empty) && !f.IsDeleted)
                .Include(f => f.Entity)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get field by ID
        public async Task<DynamicField?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicFields
                .Include(f => f.Entity)
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    (f.TenantId == TenantId || f.TenantId == string.Empty));
        }

        // ✅ Create new field
        public async Task<DynamicField> CreateAsync(DynamicField field)
        {
            field.TenantId = TenantId;
            field.CreatedByUserId = UserId;
            field.CreatedAt = DateTime.UtcNow;

            _context.DynamicFields.Add(field);
            await _context.SaveChangesAsync();

            return field;
        }

        // ✅ Update field
        public async Task<DynamicField> UpdateAsync(DynamicField field)
        {
            var existing = await _context.DynamicFields
                .FirstOrDefaultAsync(f => f.Id == field.Id && f.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Field not found or unauthorized access.");

            existing.Name = field.Name;
            existing.Label = field.Label;
            existing.DataType = field.DataType;
            existing.IsRequired = field.IsRequired;
            existing.IsVisible = field.IsVisible;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicFields.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        // ✅ Soft delete
        public async Task<bool> DeleteAsync(Guid id)
        {
            var field = await _context.DynamicFields
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == TenantId);

            if (field == null)
                return false;

            field.IsDeleted = true;
            field.DeletedAt = DateTime.UtcNow;
            field.DeletedByUserId = UserId;

            _context.DynamicFields.Update(field);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Restore soft-deleted field
        public async Task<bool> RestoreAsync(Guid id)
        {
            var field = await _context.DynamicFields
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == TenantId);

            if (field == null || !field.IsDeleted)
                return false;

            field.IsDeleted = false;
            field.DeletedAt = null;
            field.DeletedByUserId = null;

            _context.DynamicFields.Update(field);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Get all fields for specific entity
        public async Task<IEnumerable<DynamicField>> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicFields
                .Where(f =>
                    f.EntityId == entityId &&
                    (f.TenantId == TenantId || f.TenantId == string.Empty) &&
                    !f.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

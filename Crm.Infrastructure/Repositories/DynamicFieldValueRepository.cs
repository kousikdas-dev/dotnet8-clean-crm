using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicFieldValueRepository : IDynamicFieldValueRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicFieldValueRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all field values for the current tenant
        public async Task<IEnumerable<DynamicFieldValue>> GetAllAsync()
        {
            return await _context.DynamicFieldValues
                .Include(v => v.Field)
                .Include(v => v.Record)
                .Where(v => v.TenantId == TenantId)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get field value by ID
        public async Task<DynamicFieldValue?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicFieldValues
                .Include(v => v.Field)
                .Include(v => v.Record)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == TenantId);
        }

        // ✅ Create new field value
        public async Task<DynamicFieldValue> CreateAsync(DynamicFieldValue value)
        {
            value.TenantId = TenantId;
            value.CreatedByUserId = UserId;
            value.CreatedAt = DateTime.UtcNow;

            _context.DynamicFieldValues.Add(value);
            await _context.SaveChangesAsync();

            return value;
        }

        // ✅ Update existing field value
        public async Task<DynamicFieldValue> UpdateAsync(DynamicFieldValue value)
        {
            var existing = await _context.DynamicFieldValues
                .FirstOrDefaultAsync(v => v.Id == value.Id && v.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Field value not found or unauthorized access.");

            existing.Value = value.Value;
            existing.FieldName = value.FieldName;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicFieldValues.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        // ✅ Hard delete a field value
        public async Task<bool> DeleteAsync(Guid id)
        {
            var value = await _context.DynamicFieldValues
                .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == TenantId);

            if (value == null)
                return false;

            _context.DynamicFieldValues.Remove(value);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ Get all field values for a specific record
        public async Task<IEnumerable<DynamicFieldValue>> GetByRecordIdAsync(Guid recordId)
        {
            return await _context.DynamicFieldValues
                .Include(v => v.Field)
                .Where(v => v.RecordId == recordId && v.TenantId == TenantId)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get all field values for all records of a specific entity
        public async Task<IEnumerable<DynamicFieldValue>> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicFieldValues
                .Include(v => v.Field)
                .Include(v => v.Record)
                .Where(v => v.Field.EntityId == entityId && v.TenantId == TenantId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

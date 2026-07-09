using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicRecordRepository : IDynamicRecordRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicRecordRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all records for tenant
        public async Task<IEnumerable<DynamicRecord>> GetAllAsync()
        {
            return await _context.DynamicRecords
                .Where(r => r.TenantId == TenantId && !r.IsDeleted)
                .Include(r => r.Entity)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get record by ID
        public async Task<DynamicRecord?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicRecords
                .Include(r => r.Entity)
                .Include(r => r.FieldValues!)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == TenantId);
        }

        // ✅ Get all records by EntityId
        public async Task<IEnumerable<DynamicRecord>> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicRecords
                .Where(r => r.EntityId == entityId && r.TenantId == TenantId && !r.IsDeleted)
                .Include(r => r.FieldValues!)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Create new record
        public async Task<DynamicRecord> CreateAsync(DynamicRecord record)
        {
            record.TenantId = TenantId;
            record.CreatedByUserId = UserId;
            record.CreatedAt = DateTime.UtcNow;
            record.RecordKey = string.IsNullOrWhiteSpace(record.RecordKey)
                ? Guid.NewGuid().ToString()
                : record.RecordKey;

            if (record.FieldValues != null && record.FieldValues.Count > 0)
            {
                foreach (var fv in record.FieldValues)
                {
                    fv.TenantId = TenantId;
                    fv.CreatedByUserId = UserId;
                    fv.CreatedAt = DateTime.UtcNow;
                    fv.RecordId = record.Id;
                }
            }

            _context.DynamicRecords.Add(record);
            await _context.SaveChangesAsync();

            return record;
        }

        // ✅ Update existing record
        public async Task<DynamicRecord> UpdateAsync(DynamicRecord record)
        {
            var existing = await _context.DynamicRecords
                .FirstOrDefaultAsync(r => r.Id == record.Id && r.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Record not found or unauthorized access.");

            existing.DataJson = record.DataJson;
            existing.Summary = record.Summary;
            existing.PrimaryDisplayField = record.PrimaryDisplayField;
            existing.Status = record.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicRecords.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        // ✅ Soft delete record
        public async Task<bool> DeleteAsync(Guid id)
        {
            var record = await _context.DynamicRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == TenantId);

            if (record == null)
                return false;

            record.IsDeleted = true;
            record.DeletedAt = DateTime.UtcNow;
            record.DeletedByUserId = UserId;

            _context.DynamicRecords.Update(record);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ Restore soft-deleted record
        public async Task<bool> RestoreAsync(Guid id)
        {
            var record = await _context.DynamicRecords
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == TenantId);

            if (record == null || !record.IsDeleted)
                return false;

            record.IsDeleted = false;
            record.DeletedAt = null;
            record.DeletedByUserId = null;

            _context.DynamicRecords.Update(record);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Search by keyword (simple JSON or summary search)
        public async Task<IEnumerable<DynamicRecord>> SearchAsync(Guid entityId, string keyword)
        {
            return await _context.DynamicRecords
                .Where(r =>
                    r.EntityId == entityId &&
                    r.TenantId == TenantId &&
                    !r.IsDeleted &&
                    (
                        (r.Summary != null && r.Summary.Contains(keyword)) ||
                        (r.DataJson != null && r.DataJson.Contains(keyword))
                    ))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicFieldOptionRepository : IDynamicFieldOptionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicFieldOptionRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all options for current tenant
        public async Task<IEnumerable<DynamicFieldOption>> GetAllAsync()
        {
            return await _context.DynamicFieldOptions
                .Include(o => o.Field)
                .Where(o => o.TenantId == TenantId && !o.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get option by ID
        public async Task<DynamicFieldOption?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicFieldOptions
                .Include(o => o.Field)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == TenantId);
        }

        // ✅ Get options by EntityId (through field → entity)
        public async Task<IEnumerable<DynamicFieldOption>> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicFieldOptions
                .Include(o => o.Field)
                .Where(o =>
                    o.Field.EntityId == entityId &&
                    o.TenantId == TenantId &&
                    !o.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Create new option
        public async Task<DynamicFieldOption> CreateAsync(DynamicFieldOption option)
        {
            option.TenantId = TenantId;
            option.CreatedByUserId = UserId;
            option.CreatedAt = DateTime.UtcNow;

            _context.DynamicFieldOptions.Add(option);
            await _context.SaveChangesAsync();

            return option;
        }

        // ✅ Update existing option
        public async Task<DynamicFieldOption> UpdateAsync(DynamicFieldOption option)
        {
            var existing = await _context.DynamicFieldOptions
                .FirstOrDefaultAsync(o => o.Id == option.Id && o.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Field option not found or unauthorized access.");

            existing.Value = option.Value;
            existing.Label = option.Label;
            existing.Order = option.Order;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicFieldOptions.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        // ✅ Soft delete
        public async Task<bool> DeleteAsync(Guid id)
        {
            var option = await _context.DynamicFieldOptions
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == TenantId);

            if (option == null)
                return false;

            option.IsDeleted = true;
            option.DeletedAt = DateTime.UtcNow;
            option.DeletedByUserId = UserId;

            _context.DynamicFieldOptions.Update(option);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ Restore soft deleted option
        public async Task<bool> RestoreAsync(Guid id)
        {
            var option = await _context.DynamicFieldOptions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == TenantId);

            if (option == null)
                return false;

            option.IsDeleted = false;
            option.DeletedAt = null;
            option.DeletedByUserId = null;

            _context.DynamicFieldOptions.Update(option);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

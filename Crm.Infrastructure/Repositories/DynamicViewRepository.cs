using Crm.Application.Interfaces;
using Crm.Domain.Entities.Dynamic;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicViewRepository : IDynamicViewRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicViewRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all views
        public async Task<IEnumerable<DynamicView>> GetAllAsync()
        {
            return await _context.DynamicViews
                .Where(v => v.TenantId == TenantId && !v.IsDeleted)
                .Include(v => v.Entity)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get all views for an entity
        public async Task<IEnumerable<DynamicView>> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicViews
                .Where(v => v.EntityId == entityId && v.TenantId == TenantId && !v.IsDeleted)
                .Include(v => v.Entity)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get by ID
        public async Task<DynamicView?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicViews
                .Include(v => v.Entity)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == TenantId);
        }

        // ✅ Create
        public async Task<DynamicView> CreateAsync(DynamicView view)
        {
            view.TenantId = TenantId;
            view.CreatedByUserId = UserId;
            view.CreatedAt = DateTime.UtcNow;

            _context.DynamicViews.Add(view);
            await _context.SaveChangesAsync();

            return view;
        }

        // ✅ Update
        public async Task<DynamicView> UpdateAsync(DynamicView view)
        {
            var existing = await _context.DynamicViews
                .FirstOrDefaultAsync(v => v.Id == view.Id && v.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Dynamic view not found or unauthorized access.");

            existing.Name = view.Name;
            existing.ViewType = view.ViewType;
            existing.LayoutJson = view.LayoutJson;
            existing.IsDefault = view.IsDefault;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicViews.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        // ✅ Soft Delete
        public async Task<bool> DeleteAsync(Guid id)
        {
            var view = await _context.DynamicViews
                .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == TenantId);

            if (view == null)
                return false;

            view.IsDeleted = true;
            view.DeletedAt = DateTime.UtcNow;
            view.DeletedByUserId = UserId;

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Restore
        public async Task<bool> RestoreAsync(Guid id)
        {
            var view = await _context.DynamicViews
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == TenantId);

            if (view == null || !view.IsDeleted)
                return false;

            view.IsDeleted = false;
            view.DeletedAt = null;
            view.DeletedByUserId = null;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

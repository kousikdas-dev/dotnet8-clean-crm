using Crm.Application.Interfaces;
using Crm.Domain.Entities.Dynamic;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicPermissionRepository : IDynamicPermissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicPermissionRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all permissions
        public async Task<IEnumerable<DynamicPermission>> GetAllAsync()
        {
            return await _context.DynamicPermissions
                .Include(p => p.Entity)
                .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get permissions by entity ID
        public async Task<IEnumerable<DynamicPermission>> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicPermissions
                .Include(p => p.Entity)
                .Where(p =>
                    p.EntityId == entityId &&
                    p.TenantId == TenantId &&
                    !p.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get permission by ID
        public async Task<DynamicPermission?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicPermissions
                .Include(p => p.Entity)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == TenantId);
        }

        // ✅ Create new permission
        public async Task<DynamicPermission> CreateAsync(DynamicPermission permission)
        {
            permission.TenantId = TenantId;
            permission.CreatedByUserId = UserId;
            permission.CreatedAt = DateTime.UtcNow;

            _context.DynamicPermissions.Add(permission);
            await _context.SaveChangesAsync();

            return permission;
        }

        // ✅ Update permission
        public async Task<DynamicPermission> UpdateAsync(DynamicPermission permission)
        {
            var existing = await _context.DynamicPermissions
                .FirstOrDefaultAsync(p => p.Id == permission.Id && p.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Permission not found or unauthorized access.");

            existing.RoleName = permission.RoleName;
            existing.CanRead = permission.CanRead;
            existing.CanCreate = permission.CanCreate;
            existing.CanUpdate = permission.CanUpdate;
            existing.CanDelete = permission.CanDelete;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicPermissions.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        // ✅ Soft delete
        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _context.DynamicPermissions
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == TenantId);

            if (existing == null)
                return false;

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            existing.DeletedByUserId = UserId;

            _context.DynamicPermissions.Update(existing);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ Restore permission
        public async Task<bool> RestoreAsync(Guid id)
        {
            var existing = await _context.DynamicPermissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == TenantId);

            if (existing == null)
                return false;

            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedByUserId = null;

            _context.DynamicPermissions.Update(existing);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

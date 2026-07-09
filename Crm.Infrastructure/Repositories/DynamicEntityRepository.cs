using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicEntityRepository : IDynamicEntityRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicEntityRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all entities for current tenant
        public async Task<IEnumerable<DynamicEntity>> GetAllAsync()
        {
            return await _context.DynamicEntities
                // Allow "global" entities where TenantId is empty
                .Where(e => e.TenantId == TenantId || e.TenantId == string.Empty)
                .Include(e => e.Fields)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get entity by ID
        public async Task<DynamicEntity?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicEntities
                .Include(e => e.Fields)
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    (e.TenantId == TenantId || e.TenantId == string.Empty));
        }

        // ✅ Get entity by Name (Tenant-aware + Global)
        public async Task<DynamicEntity?> GetByNameAsync(string name)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // 1) Prefer tenant-owned or global (empty tenant) metadata
            var entity = await _context.DynamicEntities
                .Include(e => e.Fields)
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Name == name &&
                    (e.TenantId == tenantId || e.TenantId == string.Empty)
                );

            if (entity != null) return entity;

            // 2) Fallback: if the entity exists under a different tenant, return it as metadata.
            // This avoids "not found" when base entities were created under a different tenant.
            return await _context.DynamicEntities
                .Include(e => e.Fields)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == name);
        }


        // ✅ Create new entity
        //public async Task<DynamicEntity> CreateAsync(DynamicEntity entity)
        //{
        //    entity.TenantId = TenantId;
        //    entity.CreatedByUserId = UserId;
        //    entity.CreatedAt = DateTime.UtcNow;

        //    // ✅ CRITICAL FIX
        //    foreach (var field in entity.Fields)
        //    {
        //        field.TenantId = TenantId;
        //        field.CreatedByUserId = UserId;
        //        field.CreatedAt = DateTime.UtcNow;
        //    }

        //    _context.DynamicEntities.Add(entity);
        //    await _context.SaveChangesAsync();

        //    return entity;
        //}



        public async Task<DynamicEntity> CreateAsync(DynamicEntity entity)
        {
            entity.TenantId = TenantId;
            entity.CreatedByUserId = UserId;
            entity.CreatedAt = DateTime.UtcNow;

            _context.DynamicEntities.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        // ✅ Update entity
        public async Task<DynamicEntity> UpdateAsync(DynamicEntity entity)
        {
            var existing = await _context.DynamicEntities
                .FirstOrDefaultAsync(e => e.Id == entity.Id && e.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Dynamic entity does not belong to current tenant.");

            existing.Name = entity.Name;
            existing.DisplayName = entity.DisplayName;
            existing.Description = entity.Description;
            existing.SchemaJson = entity.SchemaJson;
            existing.IsActive = entity.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicEntities.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        // ✅ Soft Delete entity
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.DynamicEntities
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == TenantId);

            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedByUserId = UserId;

            _context.DynamicEntities.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Restore deleted entity
        public async Task<bool> RestoreAsync(Guid id)
        {
            var entity = await _context.DynamicEntities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == TenantId);

            if (entity == null || !entity.IsDeleted)
                return false;

            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedByUserId = null;

            _context.DynamicEntities.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Add field to entity
        public async Task<DynamicField> AddFieldAsync(Guid entityId, DynamicField field)
        {
            var entity = await _context.DynamicEntities
                .FirstOrDefaultAsync(e => e.Id == entityId && e.TenantId == TenantId);

            if (entity == null)
                throw new UnauthorizedAccessException("Entity not found or unauthorized.");

            field.EntityId = entityId;
            field.TenantId = TenantId;
            field.CreatedByUserId = UserId;
            field.CreatedAt = DateTime.UtcNow;

            _context.DynamicFields.Add(field);
            await _context.SaveChangesAsync();

            return field;
        }

        // ✅ Get fields for an entity
        public async Task<IEnumerable<DynamicField>> GetFieldsByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicFields
                .Where(f => f.EntityId == entityId && f.TenantId == TenantId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

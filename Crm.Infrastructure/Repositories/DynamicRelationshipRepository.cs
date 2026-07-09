using Crm.Application.Interfaces;
using Crm.Domain.Entities.Dynamic;
using Crm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Repositories
{
    public class DynamicRelationshipRepository : IDynamicRelationshipRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DynamicRelationshipRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();
        private string UserId => _tenantProvider.GetUserId();

        // ✅ Get all relationships for current tenant
        public async Task<IEnumerable<DynamicRelationship>> GetAllAsync()
        {
            return await _context.DynamicRelationships
                .Include(r => r.SourceEntity)
                .Include(r => r.TargetEntity)
                .Where(r => r.TenantId == TenantId && !r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Get relationship by ID
        public async Task<DynamicRelationship?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicRelationships
                .Include(r => r.SourceEntity)
                .Include(r => r.TargetEntity)
                .Where(r => r.TenantId == TenantId && !r.IsDeleted)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // ✅ Get all relationships for a specific entity (source OR target)
        public async Task<IEnumerable<DynamicRelationship>> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.DynamicRelationships
                .Include(r => r.SourceEntity)
                .Include(r => r.TargetEntity)
                .Where(r =>
                    r.TenantId == TenantId &&
                    !r.IsDeleted &&
                    (r.SourceEntityId == entityId || r.TargetEntityId == entityId))
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Create new relationship
        public async Task<DynamicRelationship> CreateAsync(DynamicRelationship relationship)
        {
            relationship.TenantId = TenantId;
            relationship.CreatedByUserId = UserId;
            relationship.CreatedAt = DateTime.UtcNow;

            _context.DynamicRelationships.Add(relationship);
            await _context.SaveChangesAsync();

            return relationship;
        }

        // ✅ Update relationship
        public async Task<DynamicRelationship> UpdateAsync(DynamicRelationship relationship)
        {
            var existing = await _context.DynamicRelationships
                .FirstOrDefaultAsync(r => r.Id == relationship.Id && r.TenantId == TenantId);

            if (existing == null)
                throw new UnauthorizedAccessException("Relationship not found or unauthorized access.");

            // Update fields
            existing.SourceEntityId = relationship.SourceEntityId;
            existing.TargetEntityId = relationship.TargetEntityId;
            existing.RelationType = relationship.RelationType;
            existing.SourceFieldName = relationship.SourceFieldName;
            existing.TargetFieldName = relationship.TargetFieldName;
            existing.CascadeDelete = relationship.CascadeDelete;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = UserId;

            _context.DynamicRelationships.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        // ✅ Soft delete
        public async Task<bool> DeleteAsync(Guid id)
        {
            var relationship = await _context.DynamicRelationships
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == TenantId);

            if (relationship == null)
                return false;

            relationship.IsDeleted = true;
            relationship.DeletedAt = DateTime.UtcNow;
            relationship.DeletedByUserId = UserId;

            _context.DynamicRelationships.Update(relationship);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ Restore soft deleted relationship
        public async Task<bool> RestoreAsync(Guid id)
        {
            var relationship = await _context.DynamicRelationships
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == TenantId);

            if (relationship == null || !relationship.IsDeleted)
                return false;

            relationship.IsDeleted = false;
            relationship.DeletedAt = null;
            relationship.DeletedByUserId = null;

            _context.DynamicRelationships.Update(relationship);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

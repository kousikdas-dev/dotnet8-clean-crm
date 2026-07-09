using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;

namespace Crm.Application.Interfaces
{
    public interface IDynamicPermissionRepository
    {
        Task<IEnumerable<DynamicPermission>> GetAllAsync();

        Task<DynamicPermission?> GetByIdAsync(Guid id);

        Task<IEnumerable<DynamicPermission>> GetByEntityIdAsync(Guid entityId);

        Task<DynamicPermission> CreateAsync(DynamicPermission permission);

        Task<DynamicPermission> UpdateAsync(DynamicPermission permission);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> RestoreAsync(Guid id);
    }
}

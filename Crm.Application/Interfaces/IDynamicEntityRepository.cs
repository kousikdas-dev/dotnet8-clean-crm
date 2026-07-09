using Crm.Domain.Entities;

namespace Crm.Application.Interfaces
{
    public interface IDynamicEntityRepository
    {
        Task<IEnumerable<DynamicEntity>> GetAllAsync();
        Task<DynamicEntity?> GetByIdAsync(Guid id);
        /// <summary>
        /// Get a dynamic entity by name (tenant-aware, supports global entities).
        /// </summary>
        Task<DynamicEntity?> GetByNameAsync(string name);
        Task<DynamicEntity> CreateAsync(DynamicEntity entity);
        Task<DynamicEntity> UpdateAsync(DynamicEntity entity);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> RestoreAsync(Guid id);
        Task<DynamicField> AddFieldAsync(Guid entityId, DynamicField field);
        Task<IEnumerable<DynamicField>> GetFieldsByEntityIdAsync(Guid entityId);

       
    }
}

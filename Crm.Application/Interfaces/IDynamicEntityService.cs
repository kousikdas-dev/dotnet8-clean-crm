using Crm.Application.DTOs.DynamicEntities;

namespace Crm.Application.Interfaces
{
    public interface IDynamicEntityService
    {
        /// <summary>
        /// Get all dynamic entities for the current tenant.
        /// </summary>
        Task<IEnumerable<DynamicEntityDto>> GetAllEntitiesAsync();

        /// <summary>
        /// Get a dynamic entity by its unique identifier.
        /// </summary>
        Task<DynamicEntityDto?> GetEntityByIdAsync(Guid id);

        /// <summary>
        /// Get a dynamic entity by its name (tenant-aware, supports global entities).
        /// </summary>
        Task<DynamicEntityDto?> GetEntityByNameAsync(string name);

        /// <summary>
        /// Create a new dynamic entity definition.
        /// </summary>
        Task<DynamicEntityDto> CreateEntityAsync(CreateDynamicEntityDto dto);

        /// <summary>
        /// Update an existing dynamic entity definition.
        /// </summary>
        Task<DynamicEntityDto> UpdateEntityAsync(UpdateDynamicEntityDto dto);

        /// <summary>
        /// Soft delete a dynamic entity (mark as deleted).
        /// </summary>
        Task<bool> DeleteEntityAsync(Guid id);

        /// <summary>
        /// Restore a previously soft-deleted dynamic entity.
        /// </summary>
        Task<bool> RestoreEntityAsync(Guid id);
    }
}

using Crm.Application.DTOs.DynamicEntities;
//using Crm.Application.DTOs.DynamicFields;

namespace Crm.Application.Interfaces
{
    public interface IDynamicFieldService
    {
        /// <summary>
        /// Get all dynamic fields for the current tenant.
        /// </summary>
        Task<IEnumerable<DynamicFieldDto>> GetAllFieldsAsync();

        /// <summary>
        /// Get a single dynamic field by its unique identifier.
        /// </summary>
        Task<DynamicFieldDto?> GetFieldByIdAsync(Guid id);

        /// <summary>
        /// Get all fields that belong to a specific dynamic entity.
        /// </summary>
        Task<IEnumerable<DynamicFieldDto>> GetFieldsByEntityIdAsync(Guid entityId);

        /// <summary>
        /// Create a new dynamic field for a specific entity.
        /// </summary>
        Task<DynamicFieldDto> CreateFieldAsync(CreateDynamicFieldDto dto);

        /// <summary>
        /// Create a new dynamic field under a specific entity id.
        /// </summary>
        Task<DynamicFieldDto> CreateFieldForEntityAsync(Guid entityId, CreateDynamicFieldDto dto);

        /// <summary>
        /// Update an existing dynamic field.
        /// </summary>
        Task<DynamicFieldDto> UpdateFieldAsync(UpdateDynamicFieldDto dto);

        /// <summary>
        /// Soft delete a dynamic field (mark as deleted).
        /// </summary>
        Task<bool> DeleteFieldAsync(Guid id);

        /// <summary>
        /// Restore a previously soft-deleted dynamic field.
        /// </summary>
        Task<bool> RestoreFieldAsync(Guid id);
    }
}

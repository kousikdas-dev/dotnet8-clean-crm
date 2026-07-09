using Crm.Application.DTOs.DynamicFieldValues;

namespace Crm.Application.Interfaces
{
    public interface IDynamicFieldValueService
    {
        /// <summary>
        /// Get all dynamic field values for the current tenant.
        /// </summary>
        Task<IEnumerable<DynamicFieldValueDto>> GetAllValuesAsync();

        /// <summary>
        /// Get a single field value by its unique identifier.
        /// </summary>
        Task<DynamicFieldValueDto?> GetValueByIdAsync(Guid id);

        /// <summary>
        /// Get all field values that belong to a specific record.
        /// </summary>
        Task<IEnumerable<DynamicFieldValueDto>> GetValuesByRecordIdAsync(Guid recordId);

        /// <summary>
        /// Get all field values for a specific entity.
        /// </summary>
        Task<IEnumerable<DynamicFieldValueDto>> GetValuesByEntityIdAsync(Guid entityId);

        /// <summary>
        /// Create a new dynamic field value for a record.
        /// </summary>
        Task<DynamicFieldValueDto> CreateValueAsync(CreateDynamicFieldValueDto dto);

        /// <summary>
        /// Update an existing dynamic field value.
        /// </summary>
        Task<DynamicFieldValueDto> UpdateValueAsync(UpdateDynamicFieldValueDto dto);

        /// <summary>
        /// Delete a dynamic field value permanently.
        /// </summary>
        Task<bool> DeleteValueAsync(Guid id);
    }
}

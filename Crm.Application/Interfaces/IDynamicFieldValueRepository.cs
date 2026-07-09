using Crm.Domain.Entities;

namespace Crm.Application.Interfaces
{
    public interface IDynamicFieldValueRepository
    {
        /// <summary>
        /// Get all dynamic field values for the current tenant.
        /// </summary>
        Task<IEnumerable<DynamicFieldValue>> GetAllAsync();

        /// <summary>
        /// Get a dynamic field value by its unique identifier.
        /// </summary>
        Task<DynamicFieldValue?> GetByIdAsync(Guid id);

        /// <summary>
        /// Create a new dynamic field value.
        /// </summary>
        Task<DynamicFieldValue> CreateAsync(DynamicFieldValue value);

        /// <summary>
        /// Update an existing dynamic field value.
        /// </summary>
        Task<DynamicFieldValue> UpdateAsync(DynamicFieldValue value);

        /// <summary>
        /// Delete a dynamic field value (hard delete).
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Get all field values belonging to a specific record.
        /// </summary>
        Task<IEnumerable<DynamicFieldValue>> GetByRecordIdAsync(Guid recordId);

        /// <summary>
        /// Get all field values for all records of a specific entity.
        /// </summary>
        Task<IEnumerable<DynamicFieldValue>> GetByEntityIdAsync(Guid entityId);
    }
}

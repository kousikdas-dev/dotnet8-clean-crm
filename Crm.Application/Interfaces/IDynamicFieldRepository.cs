using Crm.Domain.Entities;

namespace Crm.Application.Interfaces
{
    public interface IDynamicFieldRepository
    {
        /// <summary>
        /// Get all dynamic fields for the current tenant.
        /// </summary>
        Task<IEnumerable<DynamicField>> GetAllAsync();

        /// <summary>
        /// Get a dynamic field by its unique identifier.
        /// </summary>
        Task<DynamicField?> GetByIdAsync(Guid id);

        /// <summary>
        /// Create a new dynamic field.
        /// </summary>
        Task<DynamicField> CreateAsync(DynamicField field);

        /// <summary>
        /// Update an existing dynamic field.
        /// </summary>
        Task<DynamicField> UpdateAsync(DynamicField field);

        /// <summary>
        /// Soft delete a dynamic field (mark as deleted).
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Restore a previously soft-deleted dynamic field.
        /// </summary>
        Task<bool> RestoreAsync(Guid id);

        /// <summary>
        /// Get all fields belonging to a specific entity.
        /// </summary>
        Task<IEnumerable<DynamicField>> GetByEntityIdAsync(Guid entityId);
    }
}

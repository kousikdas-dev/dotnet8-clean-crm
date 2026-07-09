using Crm.Domain.Entities;

namespace Crm.Application.Interfaces
{
    public interface IDynamicRecordRepository
    {
        /// <summary>
        /// Get all dynamic records for the current tenant.
        /// </summary>
        Task<IEnumerable<DynamicRecord>> GetAllAsync();

        /// <summary>
        /// Get a dynamic record by its unique identifier.
        /// </summary>
        Task<DynamicRecord?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get all dynamic records belonging to a specific entity.
        /// </summary>
        Task<IEnumerable<DynamicRecord>> GetByEntityIdAsync(Guid entityId);

        /// <summary>
        /// Create a new dynamic record.
        /// </summary>
        Task<DynamicRecord> CreateAsync(DynamicRecord record);

        /// <summary>
        /// Update an existing dynamic record.
        /// </summary>
        Task<DynamicRecord> UpdateAsync(DynamicRecord record);

        /// <summary>
        /// Soft delete a dynamic record (mark as deleted).
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Restore a previously soft-deleted dynamic record.
        /// </summary>
        Task<bool> RestoreAsync(Guid id);

        /// <summary>
        /// Search records using a keyword (optional feature).
        /// </summary>
        Task<IEnumerable<DynamicRecord>> SearchAsync(Guid entityId, string keyword);
    }
}

using Crm.Application.DTOs.DynamicRecords;

namespace Crm.Application.Interfaces
{
    public interface IDynamicRecordService
    {
        /// <summary>
        /// Get all dynamic records for the current tenant.
        /// </summary>
        Task<IEnumerable<DynamicRecordDto>> GetAllRecordsAsync();

        /// <summary>
        /// Get all records that belong to a specific dynamic entity.
        /// </summary>
        Task<IEnumerable<DynamicRecordDto>> GetRecordsByEntityIdAsync(Guid entityId);

        /// <summary>
        /// Get a single dynamic record by its unique identifier.
        /// </summary>
        Task<DynamicRecordDto?> GetRecordByIdAsync(Guid id);

        /// <summary>
        /// Create a new dynamic record for a specific entity.
        /// </summary>
        Task<DynamicRecordDto> CreateRecordAsync(CreateDynamicRecordDto dto);

        /// <summary>
        /// Update an existing dynamic record.
        /// </summary>
        Task<DynamicRecordDto> UpdateRecordAsync(UpdateDynamicRecordDto dto);

        /// <summary>
        /// Soft delete a dynamic record (mark as deleted).
        /// </summary>
        Task<bool> DeleteRecordAsync(Guid id);

        /// <summary>
        /// Restore a previously soft-deleted dynamic record.
        /// </summary>
        Task<bool> RestoreRecordAsync(Guid id);

        /// <summary>
        /// Merge a duplicate Lead record into a target Lead record.
        /// </summary>
        Task<bool> MergeLeadAsync(Guid targetLeadId, Guid sourceLeadId);
    }
}

using Crm.Domain.Entities.Dynamic;

namespace Crm.Application.Interfaces
{
    public interface IDynamicRelationshipRepository
    {
        Task<IEnumerable<DynamicRelationship>> GetAllAsync();

        Task<DynamicRelationship?> GetByIdAsync(Guid id);

        Task<IEnumerable<DynamicRelationship>> GetByEntityIdAsync(Guid entityId);

        Task<DynamicRelationship> CreateAsync(DynamicRelationship relationship);

        Task<DynamicRelationship> UpdateAsync(DynamicRelationship relationship);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> RestoreAsync(Guid id);
    }
}

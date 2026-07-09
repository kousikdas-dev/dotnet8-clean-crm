using Crm.Application.DTOs.DynamicUi;
namespace Crm.Application.Interfaces
{
    public interface IDynamicRelationshipService
    {
        Task<IEnumerable<DynamicRelationshipDto>> GetAllRelationshipsAsync();

        Task<IEnumerable<DynamicRelationshipDto>> GetRelationshipsByEntityIdAsync(Guid entityId);

        Task<DynamicRelationshipDto?> GetRelationshipByIdAsync(Guid id);

        Task<DynamicRelationshipDto> CreateRelationshipAsync(CreateDynamicRelationshipDto dto);

        Task<DynamicRelationshipDto> UpdateRelationshipAsync(UpdateDynamicRelationshipDto dto);

        Task<bool> DeleteRelationshipAsync(Guid id);

        Task<bool> RestoreRelationshipAsync(Guid id);
    }
}

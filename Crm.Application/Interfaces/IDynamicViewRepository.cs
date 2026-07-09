using Crm.Domain.Entities.Dynamic;

namespace Crm.Application.Interfaces
{
    public interface IDynamicViewRepository
    {
        Task<IEnumerable<DynamicView>> GetAllAsync();

        Task<DynamicView?> GetByIdAsync(Guid id);

        Task<IEnumerable<DynamicView>> GetByEntityIdAsync(Guid entityId);

        Task<DynamicView> CreateAsync(DynamicView view);

        Task<DynamicView> UpdateAsync(DynamicView view);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> RestoreAsync(Guid id);
    }
}

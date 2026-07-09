using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;

namespace Crm.Application.Interfaces
{
    public interface IDynamicFieldOptionRepository
    {
        Task<IEnumerable<DynamicFieldOption>> GetAllAsync();
        Task<DynamicFieldOption?> GetByIdAsync(Guid id);
        Task<IEnumerable<DynamicFieldOption>> GetByEntityIdAsync(Guid entityId);
        Task<DynamicFieldOption> CreateAsync(DynamicFieldOption option);
        Task<DynamicFieldOption> UpdateAsync(DynamicFieldOption option);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> RestoreAsync(Guid id);
    }
}

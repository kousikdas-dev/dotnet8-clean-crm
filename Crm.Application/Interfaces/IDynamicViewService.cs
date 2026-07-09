using Crm.Application.DTOs.DynamicUi;
namespace Crm.Application.Interfaces
{
    public interface IDynamicViewService
    {
        Task<IEnumerable<DynamicViewDto>> GetAllViewsAsync();

        Task<IEnumerable<DynamicViewDto>> GetViewsByEntityIdAsync(Guid entityId);

        Task<DynamicViewDto?> GetViewByIdAsync(Guid id);

        Task<DynamicViewDto> CreateViewAsync(CreateDynamicViewDto dto);

        Task<DynamicViewDto> UpdateViewAsync(UpdateDynamicViewDto dto);

        Task<bool> DeleteViewAsync(Guid id);

        Task<bool> RestoreViewAsync(Guid id);
    }
}

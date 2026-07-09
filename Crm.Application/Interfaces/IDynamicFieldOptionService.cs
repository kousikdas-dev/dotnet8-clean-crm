using Crm.Application.DTOs.DynamicUi;

namespace Crm.Application.Interfaces
{
    public interface IDynamicFieldOptionService
    {        
        Task<IEnumerable<DynamicFieldOptionDto>> GetAllFieldOptionsAsync();
        
        Task<IEnumerable<DynamicFieldOptionDto>> GetFieldOptionsByEntityIdAsync(Guid entityId);
        
        Task<DynamicFieldOptionDto?> GetFieldOptionsByIdAsync(Guid id);
        
        Task<DynamicFieldOptionDto> CreateFieldOptionsAsync(CreateDynamicFieldOptionDto dto);
        
        Task<DynamicFieldOptionDto> UpdateFieldOptionsAsync(UpdateDynamicFieldOptionDto dto);
        
        Task<bool> DeleteFieldOptionsAsync(Guid id);
        
        Task<bool> RestoreFieldOptionsAsync(Guid id);
    }
}

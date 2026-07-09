using Crm.Application.DTOs.DynamicUi;
namespace Crm.Application.Interfaces
{
    public interface IDynamicPermissionService
    {
        Task<IEnumerable<DynamicPermissionDto>> GetAllPermissionsAsync();

        Task<IEnumerable<DynamicPermissionDto>> GetPermissionsByEntityIdAsync(Guid entityId);

        Task<DynamicPermissionDto?> GetPermissionByIdAsync(Guid id);

        Task<DynamicPermissionDto> CreatePermissionAsync(CreateDynamicPermissionDto dto);

        Task<DynamicPermissionDto> UpdatePermissionAsync(UpdateDynamicPermissionDto dto);

        Task<bool> DeletePermissionAsync(Guid id);

        Task<bool> RestorePermissionAsync(Guid id);
    }
}

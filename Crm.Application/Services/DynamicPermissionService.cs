using AutoMapper;
using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class DynamicPermissionService : IDynamicPermissionService
    {
        private readonly IDynamicPermissionRepository _permissionRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicPermissionService(
            IDynamicPermissionRepository permissionRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _permissionRepository = permissionRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        // ✅ Get all permissions
        public async Task<IEnumerable<DynamicPermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicPermissionDto>>(permissions);
        }

        // ✅ Get permissions by entity
        public async Task<IEnumerable<DynamicPermissionDto>> GetPermissionsByEntityIdAsync(Guid entityId)
        {
            var permissions = await _permissionRepository.GetByEntityIdAsync(entityId);
            return _mapper.Map<IEnumerable<DynamicPermissionDto>>(permissions);
        }

        // ✅ Get permission by ID
        public async Task<DynamicPermissionDto?> GetPermissionByIdAsync(Guid id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicPermissionDto?>(permission);
        }

        // ✅ Create new permission
        public async Task<DynamicPermissionDto> CreatePermissionAsync(CreateDynamicPermissionDto dto)
        {
            var permission = _mapper.Map<DynamicPermission>(dto);
            permission.TenantId = _tenantProvider.GetTenantId();
            permission.CreatedByUserId = _tenantProvider.GetUserId();
            permission.CreatedAt = DateTime.UtcNow;

            var created = await _permissionRepository.CreateAsync(permission);

            // 🧾 Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = permission.TenantId,
                EntityName = nameof(DynamicPermission),
                EntityId = permission.Id,
                ActionType = "Create",
                UserId = permission.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicPermissionDto>(created);
        }

        // ✅ Update permission
        public async Task<DynamicPermissionDto> UpdatePermissionAsync(UpdateDynamicPermissionDto dto)
        {
            var existing = await _permissionRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic Permission not found.");

            var oldData = JsonSerializer.Serialize(existing);

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _permissionRepository.UpdateAsync(existing);

            // 🧾 Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicPermission),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicPermissionDto>(updated);
        }

        // ✅ Soft delete
        public async Task<bool> DeletePermissionAsync(Guid id)
        {
            var existing = await _permissionRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            await _permissionRepository.DeleteAsync(id);

            // 🧾 Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicPermission),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }

        // ✅ Restore soft deleted permission
        public async Task<bool> RestorePermissionAsync(Guid id)
        {
            var restored = await _permissionRepository.RestoreAsync(id);

            if (restored)
            {
                await _auditLogger.LogAsync(new AuditLog
                {
                    TenantId = _tenantProvider.GetTenantId(),
                    EntityName = nameof(DynamicPermission),
                    EntityId = id,
                    ActionType = "Restore",
                    UserId = _tenantProvider.GetUserId()
                });
            }

            return restored;
        }
    }
}

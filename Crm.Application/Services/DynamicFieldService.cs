using AutoMapper;
using Crm.Application.DTOs.DynamicEntities;
//using Crm.Application.DTOs.DynamicFields;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class DynamicFieldService : IDynamicFieldService
    {
        private readonly IDynamicFieldRepository _fieldRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicFieldService(
            IDynamicFieldRepository fieldRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _fieldRepository = fieldRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DynamicFieldDto>> GetAllFieldsAsync()
        {
            var fields = await _fieldRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicFieldDto>>(fields);
        }

        public async Task<DynamicFieldDto?> GetFieldByIdAsync(Guid id)
        {
            var field = await _fieldRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicFieldDto?>(field);
        }

        public async Task<IEnumerable<DynamicFieldDto>> GetFieldsByEntityIdAsync(Guid entityId)
        {
            var fields = await _fieldRepository.GetByEntityIdAsync(entityId);
            return _mapper.Map<IEnumerable<DynamicFieldDto>>(fields);
        }

        public async Task<DynamicFieldDto> CreateFieldAsync(CreateDynamicFieldDto dto)
        {
            var field = _mapper.Map<DynamicField>(dto);
            field.TenantId = _tenantProvider.GetTenantId();
            field.CreatedByUserId = _tenantProvider.GetUserId();
            field.CreatedAt = DateTime.UtcNow;

            var created = await _fieldRepository.CreateAsync(field);

            // ✅ Audit log: Create
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = field.TenantId,
                EntityName = nameof(DynamicField),
                EntityId = field.Id,
                ActionType = "Create",
                UserId = field.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicFieldDto>(created);
        }

        public async Task<DynamicFieldDto> CreateFieldForEntityAsync(Guid entityId, CreateDynamicFieldDto dto)
        {
            var field = _mapper.Map<DynamicField>(dto);
            field.EntityId = entityId;
            field.TenantId = _tenantProvider.GetTenantId();
            field.CreatedByUserId = _tenantProvider.GetUserId();
            field.CreatedAt = DateTime.UtcNow;

            var created = await _fieldRepository.CreateAsync(field);

            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = field.TenantId,
                EntityName = nameof(DynamicField),
                EntityId = field.Id,
                ActionType = "Create",
                UserId = field.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(new { entityId, dto })
            });

            return _mapper.Map<DynamicFieldDto>(created);
        }

        public async Task<DynamicFieldDto> UpdateFieldAsync(UpdateDynamicFieldDto dto)
        {
            var existing = await _fieldRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic Field not found.");

            var oldData = JsonSerializer.Serialize(existing);

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _fieldRepository.UpdateAsync(existing);

            // ✅ Audit log: Update
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicField),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicFieldDto>(updated);
        }

        public async Task<bool> DeleteFieldAsync(Guid id)
        {
            var existing = await _fieldRepository.GetByIdAsync(id);
            if (existing == null || existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            existing.DeletedByUserId = _tenantProvider.GetUserId();

            await _fieldRepository.UpdateAsync(existing);

            // ✅ Audit log: Delete
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicField),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = existing.DeletedByUserId,
                OldValue = oldData
            });

            return true;
        }

        public async Task<bool> RestoreFieldAsync(Guid id)
        {
            var existing = await _fieldRepository.GetByIdAsync(id);
            if (existing == null || !existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedByUserId = null;

            await _fieldRepository.UpdateAsync(existing);

            // ✅ Audit log: Restore
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicField),
                EntityId = existing.Id,
                ActionType = "Restore",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }
    }
}

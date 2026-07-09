using AutoMapper;
using Crm.Application.DTOs.DynamicFieldValues;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class DynamicFieldValueService : IDynamicFieldValueService
    {
        private readonly IDynamicFieldValueRepository _valueRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicFieldValueService(
            IDynamicFieldValueRepository valueRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _valueRepository = valueRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        // ✅ Get all field values for current tenant
        public async Task<IEnumerable<DynamicFieldValueDto>> GetAllValuesAsync()
        {
            var values = await _valueRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicFieldValueDto>>(values);
        }

        // ✅ Get a field value by ID
        public async Task<DynamicFieldValueDto?> GetValueByIdAsync(Guid id)
        {
            var value = await _valueRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicFieldValueDto?>(value);
        }

        // ✅ Get all field values by Record ID
        public async Task<IEnumerable<DynamicFieldValueDto>> GetValuesByRecordIdAsync(Guid recordId)
        {
            var values = await _valueRepository.GetByRecordIdAsync(recordId);
            return _mapper.Map<IEnumerable<DynamicFieldValueDto>>(values);
        }

        // ✅ Get all field values by Entity ID
        public async Task<IEnumerable<DynamicFieldValueDto>> GetValuesByEntityIdAsync(Guid entityId)
        {
            var values = await _valueRepository.GetByEntityIdAsync(entityId);
            return _mapper.Map<IEnumerable<DynamicFieldValueDto>>(values);
        }

        // ✅ Create a new field value
        public async Task<DynamicFieldValueDto> CreateValueAsync(CreateDynamicFieldValueDto dto)
        {
            var entity = _mapper.Map<DynamicFieldValue>(dto);
            entity.TenantId = _tenantProvider.GetTenantId();
            entity.CreatedByUserId = _tenantProvider.GetUserId();
            entity.CreatedAt = DateTime.UtcNow;

            var created = await _valueRepository.CreateAsync(entity);

            // 🧾 Audit log - Create
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = entity.TenantId,
                EntityName = nameof(DynamicFieldValue),
                EntityId = entity.Id,
                ActionType = "Create",
                UserId = entity.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicFieldValueDto>(created);
        }

        // ✅ Update an existing field value
        public async Task<DynamicFieldValueDto> UpdateValueAsync(UpdateDynamicFieldValueDto dto)
        {
            var existing = await _valueRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic Field Value not found.");

            var oldData = JsonSerializer.Serialize(existing);

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _valueRepository.UpdateAsync(existing);

            // 🧾 Audit log - Update
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicFieldValue),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicFieldValueDto>(updated);
        }

        // ✅ Delete a field value
        public async Task<bool> DeleteValueAsync(Guid id)
        {
            var existing = await _valueRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            await _valueRepository.DeleteAsync(id);

            // 🧾 Audit log - Delete
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicFieldValue),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }
    }
}

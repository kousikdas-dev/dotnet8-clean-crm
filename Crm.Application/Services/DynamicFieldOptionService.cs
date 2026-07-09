using AutoMapper;
using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class DynamicFieldOptionService : IDynamicFieldOptionService
    {
        private readonly IDynamicFieldOptionRepository _optionRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicFieldOptionService(
            IDynamicFieldOptionRepository optionRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _optionRepository = optionRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        // ✅ Get all options
        public async Task<IEnumerable<DynamicFieldOptionDto>> GetAllFieldOptionsAsync()
        {
            var options = await _optionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicFieldOptionDto>>(options);
        }

        // ✅ Get options by Entity (i.e., Field.EntityId)
        public async Task<IEnumerable<DynamicFieldOptionDto>> GetFieldOptionsByEntityIdAsync(Guid entityId)
        {
            var options = await _optionRepository.GetByEntityIdAsync(entityId);
            return _mapper.Map<IEnumerable<DynamicFieldOptionDto>>(options);
        }

        // ✅ Get option by ID
        public async Task<DynamicFieldOptionDto?> GetFieldOptionsByIdAsync(Guid id)
        {
            var option = await _optionRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicFieldOptionDto?>(option);
        }

        // ✅ Create option
        public async Task<DynamicFieldOptionDto> CreateFieldOptionsAsync(CreateDynamicFieldOptionDto dto)
        {
            var entity = _mapper.Map<DynamicFieldOption>(dto);
            entity.TenantId = _tenantProvider.GetTenantId();
            entity.CreatedByUserId = _tenantProvider.GetUserId();
            entity.CreatedAt = DateTime.UtcNow;

            var created = await _optionRepository.CreateAsync(entity);

            // ✅ Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = entity.TenantId,
                EntityName = nameof(DynamicFieldOption),
                EntityId = created.Id,
                ActionType = "Create",
                UserId = entity.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicFieldOptionDto>(created);
        }

        // ✅ Update option
        public async Task<DynamicFieldOptionDto> UpdateFieldOptionsAsync(UpdateDynamicFieldOptionDto dto)
        {
            var existing = await _optionRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic Field Option not found.");

            var oldData = JsonSerializer.Serialize(existing);

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _optionRepository.UpdateAsync(existing);

            // ✅ Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicFieldOption),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicFieldOptionDto>(updated);
        }

        // ✅ Delete option
        public async Task<bool> DeleteFieldOptionsAsync(Guid id)
        {
            var existing = await _optionRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            await _optionRepository.DeleteAsync(id);

            // ✅ Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicFieldOption),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }

        // ✅ Restore deleted option
        public async Task<bool> RestoreFieldOptionsAsync(Guid id)
        {
            var existing = await _optionRepository.GetByIdAsync(id);
            if (existing == null || !existing.IsDeleted)
                return false;

            await _optionRepository.RestoreAsync(id);

            // ✅ Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicFieldOption),
                EntityId = existing.Id,
                ActionType = "Restore",
                UserId = _tenantProvider.GetUserId()
            });

            return true;
        }
    }
}

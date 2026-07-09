using AutoMapper;
using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class DynamicViewService : IDynamicViewService
    {
        private readonly IDynamicViewRepository _viewRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicViewService(
            IDynamicViewRepository viewRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _viewRepository = viewRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        // ✅ Get all views
        public async Task<IEnumerable<DynamicViewDto>> GetAllViewsAsync()
        {
            var views = await _viewRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicViewDto>>(views);
        }

        // ✅ Get views by entity
        public async Task<IEnumerable<DynamicViewDto>> GetViewsByEntityIdAsync(Guid entityId)
        {
            var views = await _viewRepository.GetByEntityIdAsync(entityId);
            return _mapper.Map<IEnumerable<DynamicViewDto>>(views);
        }

        // ✅ Get view by ID
        public async Task<DynamicViewDto?> GetViewByIdAsync(Guid id)
        {
            var view = await _viewRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicViewDto?>(view);
        }

        // ✅ Create new view
        public async Task<DynamicViewDto> CreateViewAsync(CreateDynamicViewDto dto)
        {
            var view = _mapper.Map<DynamicView>(dto);
            view.TenantId = _tenantProvider.GetTenantId();
            view.CreatedByUserId = _tenantProvider.GetUserId();
            view.CreatedAt = DateTime.UtcNow;

            var created = await _viewRepository.CreateAsync(view);

            // 🧾 Audit Log - Create
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = view.TenantId,
                EntityName = nameof(DynamicView),
                EntityId = view.Id,
                ActionType = "Create",
                UserId = view.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicViewDto>(created);
        }

        // ✅ Update existing view
        public async Task<DynamicViewDto> UpdateViewAsync(UpdateDynamicViewDto dto)
        {
            var existing = await _viewRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic View not found.");

            var oldData = JsonSerializer.Serialize(existing);

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _viewRepository.UpdateAsync(existing);

            // 🧾 Audit Log - Update
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicView),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicViewDto>(updated);
        }

        // ✅ Soft delete view
        public async Task<bool> DeleteViewAsync(Guid id)
        {
            var existing = await _viewRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            await _viewRepository.DeleteAsync(id);

            // 🧾 Audit Log - Delete
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicView),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }

        // ✅ Restore view
        public async Task<bool> RestoreViewAsync(Guid id)
        {
            var restored = await _viewRepository.RestoreAsync(id);

            if (restored)
            {
                await _auditLogger.LogAsync(new AuditLog
                {
                    TenantId = _tenantProvider.GetTenantId(),
                    EntityName = nameof(DynamicView),
                    EntityId = id,
                    ActionType = "Restore",
                    UserId = _tenantProvider.GetUserId()
                });
            }

            return restored;
        }
    }
}

using AutoMapper;
using Crm.Application.DTOs.DynamicEntities;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class DynamicEntityService : IDynamicEntityService
    {
        private readonly IDynamicEntityRepository _entityRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicEntityService(
            IDynamicEntityRepository entityRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _entityRepository = entityRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DynamicEntityDto>> GetAllEntitiesAsync()
        {
            var entities = await _entityRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicEntityDto>>(entities);
        }

        public async Task<DynamicEntityDto?> GetEntityByIdAsync(Guid id)
        {
            var entity = await _entityRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicEntityDto?>(entity);
        }

        public async Task<DynamicEntityDto?> GetEntityByNameAsync(string name)
        {
            var entity = await _entityRepository.GetByNameAsync(name);
            return _mapper.Map<DynamicEntityDto?>(entity);
        }

        public async Task<DynamicEntityDto> CreateEntityAsync(CreateDynamicEntityDto dto)
        {
            var entity = _mapper.Map<DynamicEntity>(dto);
            entity.TenantId = _tenantProvider.GetTenantId();
            entity.CreatedByUserId = _tenantProvider.GetUserId();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Category = string.IsNullOrWhiteSpace(entity.Category) ? "Sales" : entity.Category;
            entity.PrimaryField = string.IsNullOrWhiteSpace(entity.PrimaryField) ? "name" : entity.PrimaryField;

            var created = await _entityRepository.CreateAsync(entity);

            // ✅ Audit log: Create
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = entity.TenantId,
                EntityName = nameof(DynamicEntity),
                EntityId = entity.Id,
                ActionType = "Create",
                UserId = entity.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicEntityDto>(created);
        }

        public async Task<DynamicEntityDto> UpdateEntityAsync(UpdateDynamicEntityDto dto)
        {
            var existing = await _entityRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic Entity not found.");

            var oldData = JsonSerializer.Serialize(existing);

            // Map updated fields
            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _entityRepository.UpdateAsync(existing);

            // ✅ Audit log: Update
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicEntity),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicEntityDto>(updated);
        }

        public async Task<bool> DeleteEntityAsync(Guid id)
        {
            var existing = await _entityRepository.GetByIdAsync(id);
            if (existing == null || existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            // ✅ Soft delete
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            existing.DeletedByUserId = _tenantProvider.GetUserId();

            await _entityRepository.UpdateAsync(existing);

            // ✅ Audit log: Delete
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicEntity),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = existing.DeletedByUserId,
                OldValue = oldData
            });

            return true;
        }

        public async Task<bool> RestoreEntityAsync(Guid id)
        {
            var existing = await _entityRepository.GetByIdAsync(id);
            if (existing == null || !existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedByUserId = null;

            await _entityRepository.UpdateAsync(existing);

            // ✅ Audit log: Restore
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicEntity),
                EntityId = existing.Id,
                ActionType = "Restore",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }
    }
}

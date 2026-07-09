using AutoMapper;
using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class DynamicRelationshipService : IDynamicRelationshipService
    {
        private readonly IDynamicRelationshipRepository _relationshipRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicRelationshipService(
            IDynamicRelationshipRepository relationshipRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _relationshipRepository = relationshipRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        // ✅ Get all relationships
        public async Task<IEnumerable<DynamicRelationshipDto>> GetAllRelationshipsAsync()
        {
            var relationships = await _relationshipRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicRelationshipDto>>(relationships);
        }

        // ✅ Get relationships by entity (Source or Target)
        public async Task<IEnumerable<DynamicRelationshipDto>> GetRelationshipsByEntityIdAsync(Guid entityId)
        {
            var relationships = await _relationshipRepository.GetByEntityIdAsync(entityId);
            return _mapper.Map<IEnumerable<DynamicRelationshipDto>>(relationships);
        }

        // ✅ Get relationship by ID
        public async Task<DynamicRelationshipDto?> GetRelationshipByIdAsync(Guid id)
        {
            var relationship = await _relationshipRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicRelationshipDto?>(relationship);
        }

        // ✅ Create
        public async Task<DynamicRelationshipDto> CreateRelationshipAsync(CreateDynamicRelationshipDto dto)
        {
            var rel = _mapper.Map<DynamicRelationship>(dto);
            rel.TenantId = _tenantProvider.GetTenantId();
            rel.CreatedByUserId = _tenantProvider.GetUserId();
            rel.CreatedAt = DateTime.UtcNow;

            var created = await _relationshipRepository.CreateAsync(rel);

            // 🧾 Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = rel.TenantId,
                EntityName = nameof(DynamicRelationship),
                EntityId = rel.Id,
                ActionType = "Create",
                UserId = rel.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicRelationshipDto>(created);
        }

        // ✅ Update
        public async Task<DynamicRelationshipDto> UpdateRelationshipAsync(UpdateDynamicRelationshipDto dto)
        {
            var existing = await _relationshipRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic Relationship not found.");

            var oldData = JsonSerializer.Serialize(existing);

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _relationshipRepository.UpdateAsync(existing);

            // 🧾 Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicRelationship),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicRelationshipDto>(updated);
        }

        // ✅ Soft delete
        public async Task<bool> DeleteRelationshipAsync(Guid id)
        {
            var existing = await _relationshipRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            await _relationshipRepository.DeleteAsync(id);

            // 🧾 Audit Log
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicRelationship),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }

        // ✅ Restore
        public async Task<bool> RestoreRelationshipAsync(Guid id)
        {
            var restored = await _relationshipRepository.RestoreAsync(id);

            if (restored)
            {
                await _auditLogger.LogAsync(new AuditLog
                {
                    TenantId = _tenantProvider.GetTenantId(),
                    EntityName = nameof(DynamicRelationship),
                    EntityId = id,
                    ActionType = "Restore",
                    UserId = _tenantProvider.GetUserId()
                });
            }

            return restored;
        }
    }
}

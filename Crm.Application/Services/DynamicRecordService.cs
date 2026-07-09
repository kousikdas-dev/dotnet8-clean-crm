using AutoMapper;
using Crm.Application.DTOs.DynamicRecords;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crm.Application.Services
{
    public class DynamicRecordService : IDynamicRecordService
    {
        private readonly IDynamicRecordRepository _recordRepository;
        private readonly IDynamicEntityRepository _entityRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public DynamicRecordService(
            IDynamicRecordRepository recordRepository,
            IDynamicEntityRepository entityRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _recordRepository = recordRepository;
            _entityRepository = entityRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DynamicRecordDto>> GetAllRecordsAsync()
        {
            var records = await _recordRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DynamicRecordDto>>(records);
        }

        public async Task<IEnumerable<DynamicRecordDto>> GetRecordsByEntityIdAsync(Guid entityId)
        {
            var records = await _recordRepository.GetByEntityIdAsync(entityId);
            return _mapper.Map<IEnumerable<DynamicRecordDto>>(records);
        }

        public async Task<DynamicRecordDto?> GetRecordByIdAsync(Guid id)
        {
            var record = await _recordRepository.GetByIdAsync(id);
            return _mapper.Map<DynamicRecordDto?>(record);
        }

        public async Task<DynamicRecordDto> CreateRecordAsync(CreateDynamicRecordDto dto)
        {
            var record = _mapper.Map<DynamicRecord>(dto);
            record.TenantId = _tenantProvider.GetTenantId();
            record.CreatedByUserId = _tenantProvider.GetUserId();
            record.CreatedAt = DateTime.UtcNow;
            record.Status = "Active";

            var created = await _recordRepository.CreateAsync(record);

            // ✅ Audit log: Create
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = record.TenantId,
                EntityName = nameof(DynamicRecord),
                EntityId = record.Id,
                ActionType = "Create",
                UserId = record.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicRecordDto>(created);
        }

        public async Task<DynamicRecordDto> UpdateRecordAsync(UpdateDynamicRecordDto dto)
        {
            var existing = await _recordRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Dynamic Record not found.");

            // Enforce stage rules + add specific audit for Lead status/owner/score
            if (existing.Entity?.Name == "Lead")
            {
                var oldJson = ParseJsonToDictionary(existing.DataJson);
                var newJson = ParseJsonToDictionary(dto.DataJson);

                var newStatus = GetString(newJson, "leadStatus") ?? GetString(newJson, "status");
                if (string.Equals(newStatus, "Qualified", StringComparison.OrdinalIgnoreCase))
                {
                    var company = GetString(newJson, "company") ?? GetString(newJson, "name") ?? "";
                    var lastName = GetString(newJson, "lastName") ?? "";
                    var email = GetString(newJson, "email") ?? "";
                    var phone = GetString(newJson, "phone") ?? "";

                    if (string.IsNullOrWhiteSpace(company) ||
                        string.IsNullOrWhiteSpace(lastName) ||
                        (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone)))
                    {
                        throw new ArgumentException("To move to Qualified, Company + Last Name + (Email or Phone) are required.");
                    }
                }

                await LogLeadFieldChangeAsync(existing, oldJson, newJson, "leadOwner", "LeadOwner");
                await LogLeadFieldChangeAsync(existing, oldJson, newJson, "leadScore", "LeadScore");
                // status can be in leadStatus or status; log using a single semantic name
                var oldStatus = GetString(oldJson, "leadStatus") ?? GetString(oldJson, "status") ?? "";
                var nextStatus = newStatus ?? "";
                if (!string.Equals(oldStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
                {
                    await _auditLogger.LogAsync(new AuditLog
                    {
                        TenantId = existing.TenantId,
                        EntityName = "Lead",
                        EntityId = existing.Id,
                        ActionType = "Update",
                        UserId = _tenantProvider.GetUserId(),
                        PropertyName = "Status",
                        OldValue = oldStatus,
                        NewValue = nextStatus
                    });

                    // Automation: when status becomes Contacted, auto-create a follow-up task (if LeadTask entity exists)
                    if (!string.Equals(oldStatus, "Contacted", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(nextStatus, "Contacted", StringComparison.OrdinalIgnoreCase))
                    {
                        await TryAutoCreateFollowUpTaskAsync(existing.Id);
                    }
                }
            }

            var oldData = JsonSerializer.Serialize(existing, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _recordRepository.UpdateAsync(existing);

            // ✅ Audit log: Update
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicRecord),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<DynamicRecordDto>(updated);
        }

        private static Dictionary<string, JsonElement> ParseJsonToDictionary(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

                var dict = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    dict[prop.Name] = prop.Value.Clone();
                }
                return dict;
            }
            catch
            {
                return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string? GetString(Dictionary<string, JsonElement> dict, string key)
        {
            if (!dict.TryGetValue(key, out var el)) return null;
            if (el.ValueKind == JsonValueKind.String) return el.GetString();
            if (el.ValueKind == JsonValueKind.Number) return el.GetRawText();
            if (el.ValueKind == JsonValueKind.True) return "true";
            if (el.ValueKind == JsonValueKind.False) return "false";
            if (el.ValueKind == JsonValueKind.Null) return null;
            return el.ToString();
        }

        private async Task LogLeadFieldChangeAsync(
            DynamicRecord existing,
            Dictionary<string, JsonElement> oldJson,
            Dictionary<string, JsonElement> newJson,
            string jsonKey,
            string propertyName)
        {
            var oldVal = GetString(oldJson, jsonKey) ?? "";
            var newVal = GetString(newJson, jsonKey) ?? "";
            if (string.Equals(oldVal, newVal, StringComparison.OrdinalIgnoreCase)) return;

            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = "Lead",
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = _tenantProvider.GetUserId(),
                PropertyName = propertyName,
                OldValue = oldVal,
                NewValue = newVal
            });
        }

        private async Task TryAutoCreateFollowUpTaskAsync(Guid leadRecordId)
        {
            try
            {
                var taskEntity = await _entityRepository.GetByNameAsync("LeadTask");
                if (taskEntity == null) return;

                var due = DateTime.UtcNow.AddDays(2);
                var payload = new
                {
                    leadId = leadRecordId.ToString(),
                    title = "Follow up",
                    dueDate = due.ToString("yyyy-MM-dd"),
                    priority = "Medium",
                    status = "Open",
                    createdAt = DateTime.UtcNow.ToString("o")
                };

                var record = new DynamicRecord
                {
                    EntityId = taskEntity.Id,
                    DataJson = JsonSerializer.Serialize(payload),
                    Status = "Active"
                };

                await _recordRepository.CreateAsync(record);

                await _auditLogger.LogAsync(new AuditLog
                {
                    TenantId = record.TenantId,
                    EntityName = "LeadTask",
                    EntityId = record.Id,
                    ActionType = "Create",
                    UserId = _tenantProvider.GetUserId(),
                    PropertyName = "AutoCreate",
                    NewValue = $"Created follow-up task for lead {leadRecordId}"
                });
            }
            catch
            {
                // Non-critical automation; never block lead updates.
            }
        }

        public async Task<bool> DeleteRecordAsync(Guid id)
        {
            var existing = await _recordRepository.GetByIdAsync(id);
            if (existing == null || existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            existing.DeletedByUserId = _tenantProvider.GetUserId();

            await _recordRepository.UpdateAsync(existing);

            // ✅ Audit log: Delete
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicRecord),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = existing.DeletedByUserId,
                OldValue = oldData
            });

            return true;
        }

        public async Task<bool> RestoreRecordAsync(Guid id)
        {
            var existing = await _recordRepository.GetByIdAsync(id);
            if (existing == null || !existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedByUserId = null;

            await _recordRepository.UpdateAsync(existing);

            // ✅ Audit log: Restore
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(DynamicRecord),
                EntityId = existing.Id,
                ActionType = "Restore",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }

        public async Task<bool> MergeLeadAsync(Guid targetLeadId, Guid sourceLeadId)
        {
            if (targetLeadId == sourceLeadId) return false;

            var target = await _recordRepository.GetByIdAsync(targetLeadId);
            var source = await _recordRepository.GetByIdAsync(sourceLeadId);
            if (target == null || source == null) return false;
            if (target.Entity?.Name != "Lead" || source.Entity?.Name != "Lead") return false;

            var targetJson = ParseJsonToDictionary(target.DataJson);
            var sourceJson = ParseJsonToDictionary(source.DataJson);

            // Merge: fill missing/empty fields in target from source
            foreach (var kv in sourceJson)
            {
                var key = kv.Key;
                var srcVal = GetString(sourceJson, key);
                if (string.IsNullOrWhiteSpace(srcVal)) continue;

                var tgtVal = GetString(targetJson, key);
                if (string.IsNullOrWhiteSpace(tgtVal))
                {
                    targetJson[key] = kv.Value;
                }
            }

            var mergedObj = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in targetJson)
            {
                mergedObj[kv.Key] = kv.Value.ValueKind == JsonValueKind.String ? kv.Value.GetString() : kv.Value.Deserialize<object>();
            }

            target.DataJson = JsonSerializer.Serialize(mergedObj);
            await _recordRepository.UpdateAsync(target);
            await _recordRepository.DeleteAsync(sourceLeadId);

            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = target.TenantId,
                EntityName = "Lead",
                EntityId = target.Id,
                ActionType = "Merge",
                UserId = _tenantProvider.GetUserId(),
                OldValue = $"Merged from {sourceLeadId}",
                NewValue = $"Target {targetLeadId}"
            });

            return true;
        }
    }
}

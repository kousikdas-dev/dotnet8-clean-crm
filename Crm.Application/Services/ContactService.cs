using AutoMapper;
using Crm.Application.DOTs.Contact;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using System.Text.Json;

namespace Crm.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly IMapper _mapper;

        public ContactService(
            IContactRepository contactRepository,
            ITenantProvider tenantProvider,
            IAuditLogger auditLogger,
            IMapper mapper)
        {
            _contactRepository = contactRepository;
            _tenantProvider = tenantProvider;
            _auditLogger = auditLogger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ContactDto>> GetAllContactsAsync()
        {
            var contacts = await _contactRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ContactDto>>(contacts);
        }

        public async Task<ContactDto?> GetContactByIdAsync(Guid id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            return _mapper.Map<ContactDto?>(contact);
        }

        public async Task<ContactDto> CreateContactAsync(CreateContactDto dto)
        {
            var contact = _mapper.Map<Contact>(dto);
            contact.TenantId = _tenantProvider.GetTenantId();
            contact.CreatedByUserId = _tenantProvider.GetUserId();
            contact.CreatedAt = DateTime.UtcNow;

            var created = await _contactRepository.CreateAsync(contact);

            // ✅ Audit log: Create
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = contact.TenantId,
                EntityName = nameof(Contact),
                EntityId = contact.Id,
                ActionType = "Create",
                UserId = contact.CreatedByUserId,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<ContactDto>(created);
        }

        public async Task<ContactDto> UpdateContactAsync(UpdateContactDto dto)
        {
            var existing = await _contactRepository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Contact not found.");

            var oldData = JsonSerializer.Serialize(existing);

            // Map updated fields
            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = _tenantProvider.GetUserId();

            var updated = await _contactRepository.UpdateAsync(existing);

            // ✅ Audit log: Update
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(Contact),
                EntityId = existing.Id,
                ActionType = "Update",
                UserId = existing.UpdatedByUserId,
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(dto)
            });

            return _mapper.Map<ContactDto>(updated);
        }

        public async Task<bool> DeleteContactAsync(Guid id)
        {
            var existing = await _contactRepository.GetByIdAsync(id);

            if (existing == null || existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            // ✅ Soft delete instead of hard delete
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            existing.DeletedByUserId = _tenantProvider.GetUserId();

            await _contactRepository.UpdateAsync(existing);

            // ✅ Audit log: Delete
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(Contact),
                EntityId = existing.Id,
                ActionType = "Delete",
                UserId = existing.DeletedByUserId,
                OldValue = oldData
            });

            return true;
        }

        public async Task<bool> RestoreContactAsync(Guid id)
        {
            var existing = await _contactRepository.GetByIdAsync(id);

            if (existing == null || !existing.IsDeleted)
                return false;

            var oldData = JsonSerializer.Serialize(existing);

            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedByUserId = null;

            await _contactRepository.UpdateAsync(existing);

            // ✅ Audit log: Restore
            await _auditLogger.LogAsync(new AuditLog
            {
                TenantId = existing.TenantId,
                EntityName = nameof(Contact),
                EntityId = existing.Id,
                ActionType = "Restore",
                UserId = _tenantProvider.GetUserId(),
                OldValue = oldData
            });

            return true;
        }
    }
}

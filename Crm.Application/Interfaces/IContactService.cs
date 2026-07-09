using Crm.Application.DOTs.Contact;

namespace Crm.Application.Interfaces
{
    public interface IContactService
    {
        /// <summary>
        /// Get all contacts for the current tenant.
        /// </summary>
        Task<IEnumerable<ContactDto>> GetAllContactsAsync();

        /// <summary>
        /// Get a contact by its unique identifier.
        /// </summary>
        Task<ContactDto?> GetContactByIdAsync(Guid id);

        /// <summary>
        /// Create a new contact.
        /// </summary>
        Task<ContactDto> CreateContactAsync(CreateContactDto dto);

        /// <summary>
        /// Update an existing contact.
        /// </summary>
        Task<ContactDto> UpdateContactAsync(UpdateContactDto dto);

        // ✅ Soft delete instead of hard delete
        Task<bool> DeleteContactAsync(Guid id);

        // ✅ Optional: Restore a soft-deleted contact
        Task<bool> RestoreContactAsync(Guid id);
    }
}

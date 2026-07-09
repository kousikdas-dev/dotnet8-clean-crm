using Crm.Domain.Entities;

namespace Crm.Application.Interfaces
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetAllAsync();
        Task<Contact?> GetByIdAsync(Guid id);
        Task<Contact> CreateAsync(Contact contact);
        Task<Contact> UpdateAsync(Contact contact);
        Task<bool> DeleteAsync(Guid id);
    }
}

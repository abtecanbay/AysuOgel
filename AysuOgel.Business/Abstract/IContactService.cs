using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IContactService
{
    Task CreateAsync(ContactMessage message);
    Task<PagedResult<ContactMessage>> GetAllAsync(MessageFilter? filter = null);
    Task<ContactMessage?> GetByIdAsync(int id);
    Task MarkAsReadAsync(int id);
    Task DeleteAsync(int id);
}
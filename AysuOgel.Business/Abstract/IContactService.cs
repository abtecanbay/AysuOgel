using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IContactService
{
    Task CreateAsync(ContactMessage message);
    Task<List<ContactMessage>> GetAllAsync();
    Task<ContactMessage?> GetByIdAsync(int id);
    Task MarkAsReadAsync(int id);
    Task DeleteAsync(int id);
}
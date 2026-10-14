using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class ContactService : IContactService
{
    private readonly AppDbContext _db;
    public ContactService(AppDbContext db) => _db = db;

    public async Task CreateAsync(ContactMessage message)
    {
        message.CreatedAt = DateTime.UtcNow;
        message.IsRead = false;
        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync();
    }

    public Task<List<ContactMessage>> GetAllAsync() =>
        _db.ContactMessages.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<ContactMessage?> GetByIdAsync(int id) =>
        _db.ContactMessages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task MarkAsReadAsync(int id)
    {
        var entity = await _db.ContactMessages.FindAsync(id);
        if (entity is null || entity.IsRead) return;
        entity.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.ContactMessages.FindAsync(id);
        if (entity is null) return;
        _db.ContactMessages.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
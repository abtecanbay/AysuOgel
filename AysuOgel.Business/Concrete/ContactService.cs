using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
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

    public Task<PagedResult<ContactMessage>> GetAllAsync(MessageFilter? filter = null)
    {
        filter ??= new MessageFilter();
        var query = _db.ContactMessages.AsNoTracking().AsQueryable();

        if (filter.Subject is { } subject)
            query = query.Where(x => x.Subject == subject);

        var name = FilterHelper.Clean(filter.Name);
        if (name is not null)
            query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(name));

        var phone = FilterHelper.PhoneDigits(filter.Phone);
        if (phone is not null)
            query = query.Where(x => x.Phone
                .Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "")
                .Contains(phone));

        var (from, toExclusive) = FilterHelper.DateRange(filter.AppointmentFrom, filter.AppointmentTo);
        if (from is { } f)
            query = query.Where(x => x.RequestedAppointmentAt != null && x.RequestedAppointmentAt >= f);
        if (toExclusive is { } t)
            query = query.Where(x => x.RequestedAppointmentAt != null && x.RequestedAppointmentAt < t);

        query = filter.Descending
            ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id);

        return query.ToPagedAsync(filter.Page, filter.PageSize);
    }

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
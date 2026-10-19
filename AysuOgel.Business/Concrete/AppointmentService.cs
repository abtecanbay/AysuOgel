using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    private readonly IAppointmentSlotService _slots;

    public AppointmentService(AppDbContext db, IAppointmentSlotService slots)
    {
        _db = db;
        _slots = slots;
    }

    public async Task<bool> CreateRequestAsync(ContactMessage message, DateTime slotStart)
    {
        if (!await _slots.IsAvailableAsync(slotStart)) return false;

        message.CreatedAt = DateTime.UtcNow;
        message.IsRead = false;
        message.RequestedAppointmentAt = slotStart;

        // Tek SaveChanges = tek işlem: mesaj ve randevu birlikte kaydolur ya da hiç kaydolmaz.
        _db.Appointments.Add(new Appointment
        {
            SlotStart = slotStart,
            Status = AppointmentStatus.Beklemede,
            ContactMessage = message
        });

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Aynı anda başka biri aynı saati aldı.
            _db.ChangeTracker.Clear();
            return false;
        }
    }

    public Task<PagedResult<Appointment>> GetAllAsync(AppointmentFilter filter)
    {
        var query = _db.Appointments.AsNoTracking().Include(a => a.ContactMessage).AsQueryable();

        if (filter.Status is { } status)
            query = query.Where(a => a.Status == status);

        var name = FilterHelper.Clean(filter.Name);
        if (name is not null)
            query = query.Where(a => (a.ContactMessage.FirstName + " " + a.ContactMessage.LastName).Contains(name));

        var phone = FilterHelper.PhoneDigits(filter.Phone);
        if (phone is not null)
            query = query.Where(a => a.ContactMessage.Phone
                .Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "")
                .Contains(phone));

        var (from, toExclusive) = FilterHelper.DateRange(filter.From, filter.To);
        if (from is { } f) query = query.Where(a => a.SlotStart >= f);
        if (toExclusive is { } t) query = query.Where(a => a.SlotStart < t);

        // Tarih aralığı yoksa ve "geçmişi göster" kapalıysa yalnızca bugünden itibaren olanlar.
        if (from is null && toExclusive is null && !filter.All)
        {
            var since = DateTime.UtcNow.AddHours(3).Date;
            query = query.Where(a => a.SlotStart >= since);
        }

        query = filter.Descending
        ? query.OrderByDescending(a => a.SlotStart).ThenByDescending(a => a.Id)
        : query.OrderBy(a => a.SlotStart).ThenBy(a => a.Id);

        return query.ToPagedAsync(filter.Page, filter.PageSize);
    }

    public async Task<bool> SetStatusAsync(int id, AppointmentStatus status)
    {
        var entity = await _db.Appointments.FindAsync(id);
        if (entity is null) return false;

        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // İptal edilmiş randevu yeniden açılırken saat başkasına verilmişse.
            _db.ChangeTracker.Clear();
            return false;
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };
}
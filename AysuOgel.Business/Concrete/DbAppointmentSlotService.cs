using System.Globalization;
using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class DbAppointmentSlotService : IAppointmentSlotService
{
    private const int MaxDaysAhead = 60;
    private readonly AppDbContext _db;

    public DbAppointmentSlotService(AppDbContext db) => _db = db;

    // Hangi günlerin açık olduğuna artık admin'in girdiği aralıklar karar verir.
    public int[] ClosedWeekdays => Array.Empty<int>();

    // Türkiye kalıcı olarak UTC+3 kullanıyor.
    private static DateOnly TurkeyToday() =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

    public DateOnly MinDate => TurkeyToday().AddDays(1);
    public DateOnly MaxDate => TurkeyToday().AddDays(MaxDaysAhead);

    public async Task<List<SlotInfo>> GetSlotsAsync(DateOnly date)
    {
        var result = new List<SlotInfo>();
        if (date < MinDate || date > MaxDate) return result;

        var windows = await _db.AvailabilityWindows.AsNoTracking()
            .Where(w => w.Date == date)
            .ToListAsync();
        if (windows.Count == 0) return result;

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var bookedStarts = await _db.Appointments.AsNoTracking()
            .Where(a => a.SlotStart >= dayStart && a.SlotStart < dayEnd
                        && a.Status != AppointmentStatus.IptalEdildi)
            .Select(a => a.SlotStart)
            .ToListAsync();
        var booked = bookedStarts.Select(TimeOnly.FromDateTime).ToHashSet();

        var times = new SortedSet<TimeOnly>();
        foreach (var w in windows)
        {
            var step = TimeSpan.FromMinutes(w.SlotMinutes);
            if (step <= TimeSpan.Zero) continue;

            var end = w.EndTime.ToTimeSpan();
            for (var t = w.StartTime.ToTimeSpan(); t + step <= end; t += step)
                times.Add(TimeOnly.FromTimeSpan(t));
        }

        foreach (var t in times)
            result.Add(new SlotInfo(t.ToString("HH:mm", CultureInfo.InvariantCulture), !booked.Contains(t)));

        return result;
    }

    public async Task<bool> IsAvailableAsync(DateTime start)
    {
        var slots = await GetSlotsAsync(DateOnly.FromDateTime(start));
        var key = start.ToString("HH:mm", CultureInfo.InvariantCulture);
        return slots.Any(s => s.Time == key && s.IsAvailable);
    }
}
using System.Globalization;
using AysuOgel.Business.Abstract;

namespace AysuOgel.Business.Concrete;

// ÖRNEK VERİ: Gerçek doluluk bilgisi için ileride DbAppointmentSlotService yazılacak.
public class StaticAppointmentSlotService : IAppointmentSlotService
{
    private static readonly TimeOnly Open = new(10, 0);
    private static readonly TimeOnly LastStart = new(16, 30);   // 17:00'de biten son randevu
    private const int StepMinutes = 30;
    private const int MaxDaysAhead = 60;

    public int[] ClosedWeekdays => new[] { (int)DayOfWeek.Sunday };

    // Türkiye kalıcı olarak UTC+3 kullanıyor.
    private static DateOnly TurkeyToday() =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

    public DateOnly MinDate => TurkeyToday().AddDays(1);
    public DateOnly MaxDate => TurkeyToday().AddDays(MaxDaysAhead);

    public Task<List<SlotInfo>> GetSlotsAsync(DateOnly date)
    {
        var slots = new List<SlotInfo>();

        var closed = ClosedWeekdays.Contains((int)date.DayOfWeek);
        if (closed || date < MinDate || date > MaxDate)
            return Task.FromResult(slots);

        var index = 0;
        for (var t = Open; t <= LastStart; t = t.AddMinutes(StepMinutes), index++)
        {
            // Örnek doluluk: her 4 saatten biri dolu.
            var available = (date.DayOfYear + index) % 4 != 0;
            slots.Add(new SlotInfo(t.ToString("HH:mm", CultureInfo.InvariantCulture), available));
        }

        return Task.FromResult(slots);
    }

    public async Task<bool> IsAvailableAsync(DateTime start)
    {
        var slots = await GetSlotsAsync(DateOnly.FromDateTime(start));
        var key = start.ToString("HH:mm", CultureInfo.InvariantCulture);
        return slots.Any(s => s.Time == key && s.IsAvailable);
    }
}
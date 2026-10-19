using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class AvailabilityController : Controller
{
    private const int MaxRangeDays = 180;
    private readonly IAvailabilityService _availability;

    public AvailabilityController(IAvailabilityService availability) => _availability = availability;

    public async Task<IActionResult> Index() =>
        View(await _availability.GetUpcomingAsync());

    [HttpGet]
    public IActionResult Create()
    {
        var today = TurkeyToday();
        var start = today.DayOfWeek switch
        {
            DayOfWeek.Saturday => today.AddDays(2),
            DayOfWeek.Sunday => today.AddDays(1),
            _ => today
        };
        // Start is a weekday; end is Friday of that same week.
        var end = start.AddDays((int)DayOfWeek.Friday - (int)start.DayOfWeek);

        return View(new AvailabilityFormViewModel
        {
            StartDate = start,
            EndDate = end,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(18, 0),
            SlotMinutes = 30,
            WeekdaysOnly = true
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AvailabilityFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var today = TurkeyToday();
        if (!model.StartDate.HasValue || !model.EndDate.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Başlangıç ve bitiş tarihlerini seçin.");
            return View(model);
        }

        var startDate = model.StartDate.Value;
        var endDate = model.EndDate.Value;
        var start = model.StartTime!.Value;
        var end = model.EndTime!.Value;
        var slotMinutes = model.SlotMinutes!.Value;

        if (startDate < today)
            ModelState.AddModelError(nameof(model.StartDate), "Geçmiş tarih için müsaitlik eklenemez.");

        if (endDate < startDate)
            ModelState.AddModelError(nameof(model.EndDate), "Bitiş tarihi başlangıç tarihinden önce olamaz.");
        else if (endDate.DayNumber - startDate.DayNumber + 1 > MaxRangeDays)
            ModelState.AddModelError(nameof(model.EndDate), $"Tek seferde en fazla {MaxRangeDays} günlük aralık ekleyebilirsiniz.");

        if (end <= start)
            ModelState.AddModelError(nameof(model.EndTime), "Bitiş saati başlangıç saatinden sonra olmalıdır.");
        else if ((end - start).TotalMinutes < slotMinutes)
            ModelState.AddModelError(nameof(model.EndTime), "Aralık, randevu süresinden kısa olamaz.");

        if (!ModelState.IsValid) return View(model);

        var windows = new List<AvailabilityWindow>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (model.WeekdaysOnly && (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday))
                continue;

            windows.Add(new AvailabilityWindow
            {
                Date = date,
                StartTime = start,
                EndTime = end,
                SlotMinutes = slotMinutes
            });
        }

        if (windows.Count == 0)
        {
            ModelState.AddModelError(nameof(model.EndDate), "Seçilen aralıkta eklenecek gün bulunamadı.");
            return View(model);
        }

        var conflicts = new List<string>();
        foreach (var window in windows)
        {
            if (await _availability.HasOverlapAsync(window.Date, window.StartTime, window.EndTime))
                conflicts.Add(window.Date.ToString("dd.MM.yyyy"));
        }

        if (conflicts.Count > 0)
        {
            ModelState.AddModelError(string.Empty,
                $"Şu tarihlerde mevcut bir saat aralığıyla çakışma var: {string.Join(", ", conflicts)}. Hiçbir yeni aralık eklenmedi.");
            return View(model);
        }

        await _availability.CreateRangeAsync(windows);
        TempData["AvailabilitySuccess"] = $"{windows.Count} günlük müsaitlik aralığı eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var w = await _availability.GetByIdAsync(id);
        if (w is null) return NotFound();

        return View(new AvailabilityFormViewModel
        {
            Id = w.Id,
            Date = w.Date,
            StartTime = w.StartTime,
            EndTime = w.EndTime,
            SlotMinutes = w.SlotMinutes
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AvailabilityFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid) return View(model);

        if (!model.Date.HasValue)
            ModelState.AddModelError(nameof(model.Date), "Tarih seçin.");

        var date = model.Date;
        var start = model.StartTime;
        var end = model.EndTime;
        var minutes = model.SlotMinutes;

        if (date.HasValue && start.HasValue && end.HasValue && minutes.HasValue)
        {
            if (date.Value < TurkeyToday())
                ModelState.AddModelError(nameof(model.Date), "Geçmiş tarih için müsaitlik düzenlenemez.");

            if (end.Value <= start.Value)
                ModelState.AddModelError(nameof(model.EndTime), "Bitiş saati başlangıçtan sonra olmalıdır.");
            else if ((end.Value - start.Value).TotalMinutes < minutes.Value)
                ModelState.AddModelError(nameof(model.EndTime), "Aralık, randevu süresinden kısa olamaz.");
            else if (await _availability.HasOverlapAsync(date.Value, start.Value, end.Value, id))
                ModelState.AddModelError(string.Empty, "Bu tarihte bu saatlerle çakışan başka bir aralık var.");
        }

        if (!ModelState.IsValid) return View(model);

        await _availability.UpdateAsync(new AvailabilityWindow
        {
            Id = id,
            Date = model.Date!.Value,
            StartTime = model.StartTime!.Value,
            EndTime = model.EndTime!.Value,
            SlotMinutes = model.SlotMinutes!.Value
        });
        TempData["AvailabilitySuccess"] = "Müsaitlik aralığı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _availability.DeleteAsync(id);
        TempData["AvailabilitySuccess"] = "Müsaitlik aralığı silindi.";
        return RedirectToAction(nameof(Index));
    }

    private static DateOnly TurkeyToday() =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));
}
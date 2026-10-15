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
    private readonly IAvailabilityService _availability;
    public AvailabilityController(IAvailabilityService availability) => _availability = availability;

    public async Task<IActionResult> Index() =>
        View(await _availability.GetUpcomingAsync());

    public IActionResult Create() => View(new AvailabilityFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AvailabilityFormViewModel model)
    {
        await ValidateAsync(model, null);
        if (!ModelState.IsValid) return View(model);

        await _availability.CreateAsync(new AvailabilityWindow
        {
            Date = model.Date!.Value,
            StartTime = model.StartTime!.Value,
            EndTime = model.EndTime!.Value,
            SlotMinutes = model.SlotMinutes!.Value
        });
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

        await ValidateAsync(model, id);
        if (!ModelState.IsValid) return View(model);

        await _availability.UpdateAsync(new AvailabilityWindow
        {
            Id = id,
            Date = model.Date!.Value,
            StartTime = model.StartTime!.Value,
            EndTime = model.EndTime!.Value,
            SlotMinutes = model.SlotMinutes!.Value
        });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _availability.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAsync(AvailabilityFormViewModel model, int? excludeId)
    {
        if (!ModelState.IsValid) return;

        var date = model.Date!.Value;
        var start = model.StartTime!.Value;
        var end = model.EndTime!.Value;
        var minutes = model.SlotMinutes!.Value;

        if (end <= start)
        {
            ModelState.AddModelError(nameof(model.EndTime), "Bitiş saati başlangıçtan sonra olmalıdır.");
        }
        else if ((end - start).TotalMinutes < minutes)
        {
            ModelState.AddModelError(nameof(model.EndTime), "Aralık, randevu süresinden kısa olamaz.");
        }
        else if (await _availability.HasOverlapAsync(date, start, end, excludeId))
        {
            ModelState.AddModelError(string.Empty, "Bu tarihte bu saatlerle çakışan başka bir aralık var.");
        }
    }
}
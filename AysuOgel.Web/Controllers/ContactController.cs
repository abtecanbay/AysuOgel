using System.Globalization;
using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AysuOgel.Web.Controllers;

public class ContactController : Controller
{
    private readonly IContactService _contacts;
    private readonly IAppointmentSlotService _slots;

    public ContactController(IContactService contacts, IAppointmentSlotService slots)
    {
        _contacts = contacts;
        _slots = slots;
    }

    // /iletisim  veya  /iletisim?subject=RandevuTalebi
    [HttpGet("iletisim")]
    public IActionResult Index(ContactSubject? subject = null) =>
        View(new ContactFormViewModel { Subject = subject });

    // Seçilen tarihin saatleri: /iletisim/saatler?date=2026-10-18
    [HttpGet("iletisim/saatler")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Slots(string? date)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d))
            return BadRequest();

        var list = await _slots.GetSlotsAsync(d);
        return Json(list.Select(s => new { time = s.Time, available = s.IsAvailable }));
    }

    [HttpPost("iletisim"), ValidateAntiForgeryToken, EnableRateLimiting("contact")]
    public async Task<IActionResult> Index(ContactFormViewModel model)
    {
        // Bot tuzağı doluysa sessizce başarılı gibi davran, kaydetme.
        if (!string.IsNullOrWhiteSpace(model.Website))
        {
            TempData["ContactSuccess"] = SuccessText;
            return RedirectToAction(nameof(Index));
        }

        DateTime? appointmentAt = null;
        if (model.Subject == ContactSubject.RandevuTalebi)
        {
            if (!DateTime.TryParseExact(model.AppointmentSlot, "yyyy-MM-dd'T'HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            {
                ModelState.AddModelError(nameof(model.AppointmentSlot),
                    "Lütfen randevu tarihi ve saatini seçin.");
            }
            else if (!await _slots.IsAvailableAsync(start))
            {
                ModelState.AddModelError(nameof(model.AppointmentSlot),
                    "Seçtiğiniz saat müsait değil. Lütfen başka bir saat seçin.");
            }
            else
            {
                appointmentAt = start;
            }
        }

        if (!ModelState.IsValid) return View(model);

        await _contacts.CreateAsync(new ContactMessage
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            Phone = model.Phone.Trim(),
            Subject = model.Subject!.Value,
            Message = model.Message.Trim(),
            RequestedAppointmentAt = appointmentAt
        });

        TempData["ContactSuccess"] = SuccessText;
        return RedirectToAction(nameof(Index));
    }

    private const string SuccessText =
        "Teşekkürler! Talebiniz bize ulaştı, en kısa sürede sizinle iletişime geçeceğiz.";
}
using System.Globalization;
using System.Security.Claims;
using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Helpers;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AysuOgel.Web.Controllers;

public class ContactController : Controller
{
    private readonly IContactService _contacts;
    private readonly IAppointmentService _appointments;
    private readonly IAppointmentSlotService _slots;
    private readonly IMemberService _members;

    public ContactController(
        IContactService contacts,
        IAppointmentService appointments,
        IAppointmentSlotService slots,
        IMemberService members)
    {
        _contacts = contacts;
        _appointments = appointments;
        _slots = slots;
        _members = members;
    }

    // /iletisim  veya  /iletisim?subject=RandevuTalebi
    // Üye girişi varsa: /iletisim?subject=SeansDegisikligi
    [HttpGet("iletisim")]
    public async Task<IActionResult> Index(ContactSubject? subject = null)
    {
        var member = await SignedInMemberAsync();
        var model = new ContactFormViewModel
        {
            Subject = subject == ContactSubject.SeansDegisikligi && member is null ? null : subject
        };

        if (member is not null)
        {
            model.FirstName = member.FirstName;
            model.LastName = member.LastName;
            model.Email = member.Email;
            model.Phone = member.Phone;
        }
        return View(model);
    }

    /// <summary>Seans sorgulama oturumu açık ve geçerliyse üyeyi döner.</summary>
    private async Task<Member?> SignedInMemberAsync()
    {
        var auth = await HttpContext.AuthenticateAsync(MemberAuth.Scheme);
        if (!auth.Succeeded
            || !int.TryParse(auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return null;

        var member = await _members.GetByIdAsync(id);
        var stamp = auth.Principal.FindFirstValue(MemberAuth.PasswordStampClaim);
        var valid = member is not null && stamp == MemberAuth.PasswordStamp(member.PasswordHash);

        ViewData["SignedInMember"] = valid ? member : null;
        return valid ? member : null;
    }

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

        var member = await SignedInMemberAsync();
        if (model.Subject == ContactSubject.SeansDegisikligi && member is null)
            ModelState.AddModelError(nameof(model.Subject),
                "Seans saati değişikliği için önce Seans Sorgula sayfasından giriş yapın.");

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

        var message = new ContactMessage
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            Phone = model.Phone.Trim(),
            Subject = model.Subject!.Value,
            Message = model.Message.Trim(),
            MemberId = member?.Id
        };

        if (appointmentAt is null)
        {
            await _contacts.CreateAsync(message);
        }
        else if (!await _appointments.CreateRequestAsync(message, appointmentAt.Value))
        {
            // Kontrol ile kayıt arasında başka biri saati aldıysa.
            ModelState.AddModelError(nameof(model.AppointmentSlot),
                "Seçtiğiniz saat az önce doldu. Lütfen başka bir saat seçin.");
            return View(model);
        }

        TempData["ContactSuccess"] = SuccessText;
        return RedirectToAction(nameof(Index));
    }

    private const string SuccessText =
        "Teşekkürler! Talebiniz bize ulaştı, en kısa sürede sizinle iletişime geçeceğiz.";
}
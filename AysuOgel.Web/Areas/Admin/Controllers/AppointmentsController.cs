using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointments;
    public AppointmentsController(IAppointmentService appointments) => _appointments = appointments;

    public async Task<IActionResult> Index(AppointmentFilter filter)
    {
        ViewData["Filter"] = filter;
        return View(await _appointments.GetAllAsync(filter));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, AppointmentStatus status, string? returnUrl = null)
    {
        if (!Enum.IsDefined(status)) return BadRequest();

        var ok = await _appointments.SetStatusAsync(id, status);
        if (!ok)
            TempData["AppointmentError"] =
                "Durum değiştirilemedi: kayıt bulunamadı ya da bu saat başka bir randevuda dolu.";

        // Filtreli listeye geri dön.
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}
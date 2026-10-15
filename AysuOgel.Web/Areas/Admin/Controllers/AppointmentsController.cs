using AysuOgel.Business.Abstract;
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

    public async Task<IActionResult> Index(bool all = false)
    {
        ViewData["All"] = all;
        return View(await _appointments.GetAllAsync(upcomingOnly: !all));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, AppointmentStatus status, bool all = false)
    {
        if (!Enum.IsDefined(status)) return BadRequest();

        var ok = await _appointments.SetStatusAsync(id, status);
        if (!ok)
            TempData["AppointmentError"] =
                "Durum değiştirilemedi: kayıt bulunamadı ya da bu saat başka bir randevuda dolu.";

        return RedirectToAction(nameof(Index), new { all });
    }
}
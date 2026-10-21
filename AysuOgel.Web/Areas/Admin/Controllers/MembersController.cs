using System.Globalization;
using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Helpers;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MembersController : Controller
{
    private readonly IMemberService _members;
    private readonly IPackageService _packages;

    public MembersController(IMemberService members, IPackageService packages)
    {
        _members = members;
        _packages = packages;
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

    public async Task<IActionResult> Index(MemberFilter filter)
    {
        ViewData["Filter"] = filter;
        ViewData["Packages"] = await _packages.GetAllAsync();
        return View(await _members.GetAllAsync(filter));
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await BuildDetailsAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    // ---- Üye ----

    public async Task<IActionResult> Create()
    {
        ViewData["Packages"] = await _packages.GetActiveAsync();
        return View(new MemberFormViewModel { StartDate = Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberFormViewModel model)
    {
        model.Id = 0;
        await ValidateEmailAsync(model);

        Package? package = null;
        if (model.PackageId is { } packageId)
        {
            package = await _packages.GetByIdAsync(packageId);
            if (package is null)
                ModelState.AddModelError(nameof(model.PackageId), "Seçilen paket bulunamadı.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Packages"] = await _packages.GetActiveAsync();
            return View(model);
        }

        var member = await _members.CreateAsync(
            new Member
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email,
                Phone = model.Phone.Trim()
            },
            model.Password!, package!.Id, model.StartDate!.Value, model.EndDate);

        TempData["MemberSuccess"] = "Üye kaydı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = member.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member is null) return NotFound();

        return View(new MemberFormViewModel
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Email = member.Email,
            Phone = member.Phone
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MemberFormViewModel model)
    {
        if (id != model.Id || id == 0) return BadRequest();
        await ValidateEmailAsync(model);
        if (!ModelState.IsValid) return View(model);

        await _members.UpdateAsync(
            new Member
            {
                Id = model.Id,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email,
                Phone = model.Phone.Trim()
            },
            model.Password);

        TempData["MemberSuccess"] = string.IsNullOrEmpty(model.Password)
            ? "Üye bilgileri güncellendi."
            : "Üye bilgileri ve şifresi güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _members.DeleteAsync(id);
        TempData["MemberSuccess"] = "Üye ve tüm paket / seans kayıtları silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateEmailAsync(MemberFormViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Email)
            && await _members.EmailExistsAsync(model.Email, model.IsNew ? null : model.Id))
            ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresiyle kayıtlı başka bir üye var.");
    }

    // ---- Satın alınan paketler ----

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPackage(int id, [Bind(Prefix = "NewPackage")] MemberPackageFormViewModel model)
    {
        string? error;
        if (model.PackageId is null)
            error = "Paket seçimi zorunludur.";
        else if (!ModelState.IsValid)
            error = FirstError();
        else
            error = await _members.AddPackageAsync(id, model.PackageId.Value, model.StartDate!.Value, model.EndDate);

        SetResult(error, "Paket üyeye eklendi.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> EditPackage(int id)
    {
        var memberPackage = await _members.GetMemberPackageAsync(id);
        if (memberPackage is null) return NotFound();

        ViewData["MemberPackage"] = memberPackage;
        return View(new MemberPackageFormViewModel
        {
            Id = memberPackage.Id,
            MemberId = memberPackage.MemberId,
            PackageId = memberPackage.PackageId,
            StartDate = memberPackage.StartDate,
            EndDate = memberPackage.EndDate
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPackage(int id, MemberPackageFormViewModel model)
    {
        var memberPackage = await _members.GetMemberPackageAsync(id);
        if (memberPackage is null) return NotFound();

        if (ModelState.IsValid)
        {
            var error = await _members.UpdatePackageAsync(id, model.StartDate!.Value, model.EndDate);
            if (error is null)
            {
                TempData["MemberSuccess"] = "Paket tarihleri güncellendi.";
                return RedirectToAction(nameof(Details), null, new { id = memberPackage.MemberId }, $"package-{id}");
            }
            ModelState.AddModelError(string.Empty, error);
        }

        ViewData["MemberPackage"] = memberPackage;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var memberPackage = await _members.GetMemberPackageAsync(id);
        if (memberPackage is null) return NotFound();

        await _members.DeletePackageAsync(id);
        TempData["MemberSuccess"] = "Paket kaydı ve seansları silindi.";
        return RedirectToAction(nameof(Details), new { id = memberPackage.MemberId });
    }

    // ---- Seanslar ----

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSession(SessionFormViewModel model)
    {
        var memberPackage = await _members.GetMemberPackageAsync(model.MemberPackageId);
        if (memberPackage is null) return NotFound();

        string? error;
        if (!ModelState.IsValid)
            error = FirstError();
        else if (await UnconfirmedConflictsAsync(model) is { } conflictError)
            error = conflictError;
        else
            error = await _members.AddSessionAsync(ToEntity(model));

        SetResult(error, $"{model.ScheduledAt:dd.MM.yyyy HH:mm} seansı kaydedildi.");
        return BackToPackage(memberPackage);
    }

    // Seans formları kaydetmeden önce bunu çağırır; çakışma varsa açılır pencerede onay istenir.
    // /Admin/Members/SessionConflicts?at=2026-10-26T13:20&exceptId=5
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> SessionConflicts(string? at, int? exceptId)
    {
        if (!DateTime.TryParseExact(at, "yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var scheduledAt))
            return BadRequest();

        var conflicts = await _members.GetConflictsAsync(scheduledAt, exceptId);
        return Json(conflicts.Select(c => new
        {
            memberId = c.MemberId,
            memberName = c.MemberName,
            packageName = c.PackageName,
            sessionNumber = c.SessionNumber,
            scheduledAt = c.ScheduledAt.ToString("dd.MM.yyyy HH:mm"),
            minutesApart = (int)Math.Round((c.ScheduledAt - scheduledAt).TotalMinutes),
            status = c.Status.DisplayName(),
            detailsUrl = Url.Action(nameof(Details), new { id = c.MemberId })
        }));
    }

    public async Task<IActionResult> EditSession(int id)
    {
        var session = await _members.GetSessionAsync(id);
        if (session is null) return NotFound();

        ViewData["Session"] = session;
        return View(new SessionFormViewModel
        {
            Id = session.Id,
            MemberPackageId = session.MemberPackageId,
            ScheduledAt = session.ScheduledAt,
            Status = session.Status,
            Note = session.Note
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSession(int id, SessionFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        var session = await _members.GetSessionAsync(id);
        if (session is null) return NotFound();

        if (ModelState.IsValid)
        {
            model.MemberPackageId = session.MemberPackageId;

            // Yalnızca not / numara değiştiyse çakışma tekrar sorulmaz.
            var timeChanged = model.ScheduledAt != session.ScheduledAt
                || session.Status == SessionStatus.IptalEdildi;
            var error = timeChanged ? await UnconfirmedConflictsAsync(model, id) : null;
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                ViewData["Session"] = session;
                return View(model);
            }

            error = await _members.UpdateSessionAsync(ToEntity(model));
            if (error is null)
            {
                TempData["MemberSuccess"] = $"{model.ScheduledAt:dd.MM.yyyy HH:mm} seansı güncellendi.";
                return BackToPackage(session.MemberPackage);
            }
            ModelState.AddModelError(string.Empty, error);
        }

        ViewData["Session"] = session;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetSessionStatus(int id, SessionStatus status)
    {
        var session = await _members.GetSessionAsync(id);
        if (session is null) return NotFound();

        var error = await _members.SetSessionStatusAsync(id, status);
        SetResult(error, $"{session.ScheduledAt:dd.MM.yyyy HH:mm} seansı \"{status.DisplayName()}\" olarak işaretlendi.");
        return BackToPackage(session.MemberPackage);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var session = await _members.GetSessionAsync(id);
        if (session is null) return NotFound();

        await _members.DeleteSessionAsync(id);
        TempData["MemberSuccess"] = $"{session.ScheduledAt:dd.MM.yyyy HH:mm} seans kaydı silindi.";
        return BackToPackage(session.MemberPackage);
    }

    // ---- Yardımcılar ----

    private async Task<MemberDetailsViewModel?> BuildDetailsAsync(int id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member is null) return null;

        member.Packages = member.Packages
            .OrderByDescending(p => p.StartDate).ThenByDescending(p => p.Id)
            .ToList();

        return new MemberDetailsViewModel
        {
            Member = member,
            AvailablePackages = await _packages.GetActiveAsync(),
            NewPackage = new MemberPackageFormViewModel { MemberId = id, StartDate = Today }
        };
    }

    /// <summary>
    /// Açılır penceredeki onay atlanırsa (ör. JavaScript kapalı) kayıt yapılmaz; çakışanlar mesajda listelenir.
    /// </summary>
    private async Task<string?> UnconfirmedConflictsAsync(SessionFormViewModel model, int? exceptSessionId = null)
    {
        if (model.ConfirmConflicts || model.Status == SessionStatus.IptalEdildi) return null;

        var conflicts = await _members.GetConflictsAsync(model.ScheduledAt!.Value, exceptSessionId);
        if (conflicts.Count == 0) return null;

        var list = string.Join("; ", conflicts.Select(c =>
            $"{c.MemberName} ({c.PackageName}, {c.ScheduledAt:dd.MM.yyyy HH:mm})"));
        return $"Seans kaydedilmedi: 1 saat içinde {conflicts.Count} başka seans var — {list}. " +
               "Kapasite yeterliyse çakışma uyarısını onaylayarak tekrar kaydedin.";
    }

    private static MemberSession ToEntity(SessionFormViewModel model) => new()
    {
        Id = model.Id,
        MemberPackageId = model.MemberPackageId,
        ScheduledAt = model.ScheduledAt!.Value,
        Status = model.Status,
        Note = model.Note
    };

    private IActionResult BackToPackage(MemberPackage memberPackage) =>
        RedirectToAction(nameof(Details), null, new { id = memberPackage.MemberId }, $"package-{memberPackage.Id}");

    private void SetResult(string? error, string success)
    {
        if (error is null)
            TempData["MemberSuccess"] = success;
        else
            TempData["MemberError"] = error;
    }

    private string FirstError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Form bilgilerini kontrol edin.";
}

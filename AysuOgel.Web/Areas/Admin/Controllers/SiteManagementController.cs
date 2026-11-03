using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Helpers;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

/// <summary>Sayfa yönetimi: site başlığı, logo, favicon ve yönetici hesapları.</summary>
[Area("Admin")]
[Authorize]
public class SiteManagementController : Controller
{
    private const long MaxRequestBytes = ImageUploadHelper.MaxImageBytes * 2 + 256 * 1024;

    private readonly ISiteSettingsService _settings;
    private readonly IAdminUserService _admins;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _config;
    private readonly IAdminPageService _pages;

    public SiteManagementController(
        ISiteSettingsService settings,
        IAdminUserService admins,
        IAdminPageService pages,
        IWebHostEnvironment environment,
        IConfiguration config)
    {
        _settings = settings;
        _admins = admins;
        _pages = pages;
        _environment = environment;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Index() =>
        View(await BuildDashboardAsync(await _settings.GetAsync()));

    // ---------------------------------------------------------------- Genel ayarlar

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Settings(
        [Bind("SiteTitle,BrandName,LogoUrl,FaviconUrl")] SiteSettings model,
        IFormFile? logoFile,
        IFormFile? faviconFile,
        bool removeLogo,
        bool removeFavicon)
    {
        model.Id = 1;

        if (removeLogo) model.LogoUrl = null;
        if (removeFavicon) model.FaviconUrl = null;

        if (logoFile is { Length: > 0 })
        {
            var path = await ImageUploadHelper.SaveImageAsync(logoFile, _environment.WebRootPath);
            if (path is null)
                ModelState.AddModelError("LogoFile", "Logo için yalnızca JPG, PNG veya WEBP yükleyebilirsiniz. En fazla 5 MB.");
            else
                model.LogoUrl = path;
        }

        if (faviconFile is { Length: > 0 })
        {
            var path = await ImageUploadHelper.SaveImageAsync(faviconFile, _environment.WebRootPath, allowIco: true);
            if (path is null)
                ModelState.AddModelError("FaviconFile", "Favicon için yalnızca ICO, PNG, JPG veya WEBP yükleyebilirsiniz. En fazla 5 MB.");
            else
                model.FaviconUrl = path;
        }

        if (!ModelState.IsValid)
            return View(nameof(Index), await BuildDashboardAsync(model));

        await _settings.UpdateAsync(model);
        TempData["SiteSuccess"] = "Site ayarları güncellendi.";
        return RedirectToAction(nameof(Index), new { section = "settings" });
    }

    // ---------------------------------------------------------------- Yöneticiler

    [HttpGet]
    public async Task<IActionResult> CreateAdmin() =>
        await AdminFormViewAsync(new AdminUserFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(AdminUserFormViewModel model)
    {
        model.Id = 0;
        if (string.IsNullOrEmpty(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Yeni yönetici için şifre zorunludur.");

        await ValidateUsernameAsync(model);
        if (!ModelState.IsValid) return await AdminFormViewAsync(model);

        var user = new AdminUser
        {
            Username = model.Username,
            FullName = model.FullName,
            Email = model.Email,
            IsActive = model.IsActive
        };
        await _admins.CreateAsync(user, model.Password!);
        await _pages.SetPagesAsync(user.Id, model.PageIds);

        TempData["SiteSuccess"] = $"{model.FullName} yönetici olarak eklendi.";
        return RedirectToAction(nameof(Index), new { section = "admins" });
    }

    [HttpGet]
    public async Task<IActionResult> EditAdmin(int id)
    {
        var user = await _admins.GetByIdAsync(id);
        if (user is null) return NotFound();

        return await AdminFormViewAsync(new AdminUserFormViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            PageIds = await _pages.GetPageIdsAsync(user.Id)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAdmin(int id, AdminUserFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (await _admins.GetByIdAsync(id) is null) return NotFound();

        var isSelf = User.GetAdminUserId() == id;
        if (!model.IsActive && isSelf)
            ModelState.AddModelError(nameof(model.IsActive), "Kendi hesabınızı pasife alamazsınız.");

        // Kendi Sayfa Yönetimi yetkisini kaldıran yönetici bu sayfaya bir daha giremez.
        if (isSelf)
        {
            var selfPage = (await _pages.GetAllCachedAsync())
                .FirstOrDefault(p => p.Controller == "SiteManagement");
            if (selfPage is not null && !model.PageIds.Contains(selfPage.Id))
                ModelState.AddModelError(nameof(model.PageIds), "Kendi hesabınızdan Sayfa Yönetimi yetkisini kaldıramazsınız.");
        }

        await ValidateUsernameAsync(model);
        if (!ModelState.IsValid) return await AdminFormViewAsync(model);

        await _admins.UpdateAsync(new AdminUser
        {
            Id = model.Id,
            Username = model.Username,
            FullName = model.FullName,
            Email = model.Email,
            IsActive = model.IsActive
        }, model.Password);
        await _pages.SetPagesAsync(model.Id, model.PageIds);

        TempData["SiteSuccess"] = $"{model.FullName} güncellendi.";
        return RedirectToAction(nameof(Index), new { section = "admins" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        if (User.GetAdminUserId() == id)
        {
            TempData["SiteError"] = "Kendi hesabınızı silemezsiniz.";
            return RedirectToAction(nameof(Index), new { section = "admins" });
        }

        await _admins.DeleteAsync(id);
        _pages.InvalidateUser(id);
        TempData["SiteSuccess"] = "Yönetici silindi.";
        return RedirectToAction(nameof(Index), new { section = "admins" });
    }

    // ---------------------------------------------------------------- Yardımcılar

    private async Task ValidateUsernameAsync(AdminUserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username)) return;

        var systemUser = _config["Admin:Username"];
        if (!string.IsNullOrEmpty(systemUser) &&
            string.Equals(model.Username.Trim(), systemUser, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Username), "Bu kullanıcı adı ana yönetici hesabına ayrılmıştır.");
            return;
        }

        if (await _admins.UsernameExistsAsync(model.Username, model.IsNew ? null : model.Id))
            ModelState.AddModelError(nameof(model.Username), "Bu kullanıcı adı zaten kullanılıyor.");
    }

    private async Task<IActionResult> AdminFormViewAsync(AdminUserFormViewModel model)
    {
        model.AvailablePages = await _pages.GetAllCachedAsync();
        return View("AdminForm", model);
    }

    private async Task<SiteManagementViewModel> BuildDashboardAsync(SiteSettings settings) => new()
    {
        Settings = settings,
        Admins = await _admins.GetAllAsync(),
        SystemAdminUsername = _config["Admin:Username"]
    };
}

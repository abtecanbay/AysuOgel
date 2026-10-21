using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

/// <summary>Paket listesi Kampanyalar sayfasındaki "Paketler" akordeonunda gösterilir.</summary>
[Area("Admin")]
[Authorize]
public class PackagesController : Controller
{
    private readonly IPackageService _packages;

    public PackagesController(IPackageService packages) => _packages = packages;

    public IActionResult Index() => BackToList();

    public IActionResult Create() => View(new Package { IsActive = true, SessionCount = 10 });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,Description,SessionCount,Price,ValidityDays,IsActive,DisplayOrder")] Package model)
    {
        if (!ModelState.IsValid) return View(model);

        await _packages.CreateAsync(model);
        TempData["CampaignSuccess"] = "Paket oluşturuldu.";
        return BackToList();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var package = await _packages.GetByIdAsync(id);
        return package is null ? NotFound() : View(package);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Description,SessionCount,Price,ValidityDays,IsActive,DisplayOrder")] Package model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        await _packages.UpdateAsync(model);
        TempData["CampaignSuccess"] = "Paket güncellendi.";
        return BackToList();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _packages.DeleteAsync(id))
            TempData["CampaignSuccess"] = "Paket silindi.";
        else
            TempData["CampaignError"] = "Bu paket üyelere satıldığı için silinemez. Satıştan kaldırmak için pasife alabilirsiniz.";
        return BackToList();
    }

    private IActionResult BackToList() =>
        RedirectToAction("Index", "Campaigns", new { area = "Admin", section = "packages" });
}

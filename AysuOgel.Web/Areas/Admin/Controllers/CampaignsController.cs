using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CampaignsController : Controller
{
    private readonly ICampaignService _campaigns;
    private readonly IWebHostEnvironment _environment;

    public CampaignsController(ICampaignService campaigns, IWebHostEnvironment environment)
    {
        _campaigns = campaigns;
        _environment = environment;
    }

    public async Task<IActionResult> Index() =>
        View(await _campaigns.GetAllAsync());

    public IActionResult Create() => View(new Campaign
    {
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3)),
        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3)).AddDays(7),
        IsActive = true
    });

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(ImageUploadHelper.MaxImageBytes + 256 * 1024)]
    public async Task<IActionResult> Create(
        [Bind("Title,ShortDescription,Description,ImageUrl,StartDate,EndDate,IsActive,DisplayOrder")] Campaign model,
        IFormFile? imageFile)
    {
        if (imageFile is { Length: > 0 })
        {
            var path = await ImageUploadHelper.SaveImageAsync(imageFile, _environment.WebRootPath);
            if (path is null)
                ModelState.AddModelError("ImageFile", "Yalnızca geçerli JPG, PNG veya WEBP görselleri yükleyebilirsiniz. En fazla 5 MB.");
            else
                model.ImageUrl = path;
        }

        if (!ModelState.IsValid) return View(model);

        await _campaigns.CreateAsync(model);
        TempData["CampaignSuccess"] = "Kampanya oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var campaign = await _campaigns.GetByIdAsync(id);
        return campaign is null ? NotFound() : View(campaign);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(ImageUploadHelper.MaxImageBytes + 256 * 1024)]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,ShortDescription,Description,ImageUrl,StartDate,EndDate,IsActive,DisplayOrder")] Campaign model,
        IFormFile? imageFile)
    {
        if (id != model.Id) return BadRequest();

        if (imageFile is { Length: > 0 })
        {
            var path = await ImageUploadHelper.SaveImageAsync(imageFile, _environment.WebRootPath);
            if (path is null)
                ModelState.AddModelError("ImageFile", "Yalnızca geçerli JPG, PNG veya WEBP görselleri yükleyebilirsiniz. En fazla 5 MB.");
            else
                model.ImageUrl = path;
        }

        if (!ModelState.IsValid) return View(model);

        await _campaigns.UpdateAsync(model);
        TempData["CampaignSuccess"] = "Kampanya güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _campaigns.DeleteAsync(id);
        TempData["CampaignSuccess"] = "Kampanya silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> PageSettings() =>
        View(await _campaigns.GetPageSettingsAsync());

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(ImageUploadHelper.MaxImageBytes + 256 * 1024)]
    public async Task<IActionResult> PageSettings(
        [Bind("BannerImageUrl,IntroTitle,IntroText")] CampaignPageSettings model,
        IFormFile? bannerFile)
    {
        model.Id = 1;

        if (bannerFile is { Length: > 0 })
        {
            var path = await ImageUploadHelper.SaveImageAsync(bannerFile, _environment.WebRootPath);
            if (path is null)
                ModelState.AddModelError("BannerFile", "Yalnızca geçerli JPG, PNG veya WEBP görselleri yükleyebilirsiniz. En fazla 5 MB.");
            else
                model.BannerImageUrl = path;
        }

        if (!ModelState.IsValid) return View(model);

        await _campaigns.UpdatePageSettingsAsync(model);
        TempData["CampaignSettingsSuccess"] = "Kampanya sayfası başlığı güncellendi.";
        return RedirectToAction(nameof(PageSettings));
    }
}
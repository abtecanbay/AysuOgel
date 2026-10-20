using AysuOgel.Business.Abstract;
using AysuOgel.Business.Concrete;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Helpers;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class AboutPageController : Controller
{
    // Tek seferde en fazla 10 görsel (her biri en fazla 5 MB) + form alanları.
    private const int MaxFilesPerUpload = 10;
    private const long MaxRequestBytes = ImageUploadHelper.MaxImageBytes * MaxFilesPerUpload + 256 * 1024;
    private const string InvalidImageText =
        "Yalnızca geçerli JPG, PNG veya WEBP görselleri yükleyebilirsiniz. En fazla 5 MB.";

    private readonly IAboutPageService _about;
    private readonly IWebHostEnvironment _environment;

    public AboutPageController(IAboutPageService about, IWebHostEnvironment environment)
    {
        _about = about;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new AboutPageDashboardViewModel
        {
            Settings = await _about.GetSettingsAsync(),
            Blocks = await _about.GetAllBlocksAsync()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings([Bind("Title,Intro")] AboutPageSettings model)
    {
        model.Id = 1;

        if (!ModelState.IsValid)
        {
            var dashboard = new AboutPageDashboardViewModel
            {
                Settings = model,
                Blocks = await _about.GetAllBlocksAsync()
            };
            return View(nameof(Index), dashboard);
        }

        await _about.UpdateSettingsAsync(model);
        TempData["AboutSuccess"] = "Sayfa başlığı ve giriş metni güncellendi.";
        return RedirectToAction(nameof(Index), new { section = "settings" });
    }

    public async Task<IActionResult> Create()
    {
        var blocks = await _about.GetAllBlocksAsync();
        return View(new AboutBlock
        {
            Layout = AboutBlockLayout.ImageLeft,
            DisplayOrder = blocks.Count == 0 ? 1 : blocks.Max(x => x.DisplayOrder) + 1
        });
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Create(
        [Bind("Slug,Title,Text,Layout,DisplayOrder,IsActive,Images")] AboutBlock model,
        int[]? removeImages,
        List<IFormFile>? imageFiles)
    {
        model.Images.ForEach(i => i.Id = 0);   // yeni blokta mevcut görsel olamaz

        await PrepareAsync(model, imageFiles);
        if (!ModelState.IsValid)
        {
            ViewData["RemoveImages"] = removeImages;
            return View(model);
        }

        ApplyRemovals(model, removeImages);
        await _about.CreateBlockAsync(model);
        TempData["AboutSuccess"] = "Blok eklendi.";
        return RedirectToAction(nameof(Index), new { section = "blocks" });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _about.GetBlockByIdAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Slug,Title,Text,Layout,DisplayOrder,IsActive,Images")] AboutBlock model,
        int[]? removeImages,
        List<IFormFile>? imageFiles)
    {
        if (id != model.Id) return BadRequest();

        await PrepareAsync(model, imageFiles);
        if (!ModelState.IsValid)
        {
            ViewData["RemoveImages"] = removeImages;
            return View(model);
        }

        ApplyRemovals(model, removeImages);
        await _about.UpdateBlockAsync(model);
        TempData["AboutSuccess"] = "Blok güncellendi.";
        return RedirectToAction(nameof(Index), new { section = "blocks" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _about.DeleteBlockAsync(id);
        TempData["AboutSuccess"] = "Blok silindi.";
        return RedirectToAction(nameof(Index), new { section = "blocks" });
    }

    private async Task PrepareAsync(AboutBlock model, List<IFormFile>? imageFiles)
    {
        await ApplySlugAsync(model);

        var files = imageFiles?.Where(f => f is { Length: > 0 }).ToList() ?? [];
        if (files.Count > MaxFilesPerUpload)
        {
            ModelState.AddModelError("ImageFiles", $"Tek seferde en fazla {MaxFilesPerUpload} görsel yükleyebilirsiniz.");
            return;
        }

        // Yeni görseller listenin sonuna eklenir; form hatayla dönerse kaybolmasınlar diye hemen kaydedilir.
        var nextOrder = model.Images.Count == 0 ? 1 : model.Images.Max(i => i.DisplayOrder) + 1;
        foreach (var file in files)
        {
            var path = await ImageUploadHelper.SaveImageAsync(file, _environment.WebRootPath);
            if (path is null)
            {
                ModelState.AddModelError("ImageFiles", $"{file.FileName}: {InvalidImageText}");
                continue;
            }

            model.Images.Add(new AboutBlockImage { Url = path, DisplayOrder = nextOrder++ });
        }
    }

    private async Task ApplySlugAsync(AboutBlock model)
    {
        int? excludeId = model.Id > 0 ? model.Id : null;

        if (string.IsNullOrWhiteSpace(model.Slug))
        {
            // Boş bırakılırsa başlıktan üretilir; çakışırsa sonuna -2, -3 ... eklenir.
            var baseSlug = SlugHelper.ToSlug(model.Title);
            if (baseSlug.Length > SlugHelper.MaxLength - 4) baseSlug = baseSlug[..(SlugHelper.MaxLength - 4)].TrimEnd('-');
            var slug = baseSlug;
            for (var n = 2; await _about.SlugExistsAsync(slug, excludeId); n++)
                slug = $"{baseSlug}-{n}";

            model.Slug = slug;
            ModelState.Remove(nameof(model.Slug));
            return;
        }

        model.Slug = model.Slug.Trim();
        if (await _about.SlugExistsAsync(model.Slug, excludeId))
            ModelState.AddModelError(nameof(model.Slug), "Bu slug başka bir blokta kullanılıyor.");
    }

    private static void ApplyRemovals(AboutBlock model, int[]? removeImages)
    {
        if (removeImages is not { Length: > 0 }) return;
        model.Images = model.Images.Where((_, index) => !removeImages.Contains(index)).ToList();
    }
}

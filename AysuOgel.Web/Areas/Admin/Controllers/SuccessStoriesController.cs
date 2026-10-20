using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class SuccessStoriesController : Controller
{
    // İki fotoğraf (her biri en fazla 5 MB) + form alanları.
    private const long MaxRequestBytes = ImageUploadHelper.MaxImageBytes * 2 + 256 * 1024;
    private const string InvalidImageText =
        "Yalnızca geçerli JPG, PNG veya WEBP görselleri yükleyebilirsiniz. En fazla 5 MB.";

    private readonly ISuccessStoryService _stories;
    private readonly IWebHostEnvironment _environment;

    public SuccessStoriesController(ISuccessStoryService stories, IWebHostEnvironment environment)
    {
        _stories = stories;
        _environment = environment;
    }

    public async Task<IActionResult> Index() =>
        View(await _stories.GetAllAsync());

    public IActionResult Create() => View(new SuccessStory());

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Create(
        [Bind("Description,DisplayOrder,IsActive")] SuccessStory model,
        IFormFile? beforeFile,
        IFormFile? afterFile)
    {
        await ApplyUploadsAsync(model, beforeFile, afterFile);
        if (!ModelState.IsValid) return View(model);

        await _stories.CreateAsync(model);
        TempData["StorySuccess"] = "Başarı kaydı eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _stories.GetByIdAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,BeforeImageUrl,AfterImageUrl,Description,DisplayOrder,IsActive")] SuccessStory model,
        IFormFile? beforeFile,
        IFormFile? afterFile)
    {
        if (id != model.Id) return BadRequest();

        await ApplyUploadsAsync(model, beforeFile, afterFile);
        if (!ModelState.IsValid) return View(model);

        await _stories.UpdateAsync(model);
        TempData["StorySuccess"] = "Başarı kaydı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _stories.DeleteAsync(id);
        TempData["StorySuccess"] = "Başarı kaydı silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ApplyUploadsAsync(SuccessStory model, IFormFile? beforeFile, IFormFile? afterFile)
    {
        var before = await TryUploadAsync(beforeFile, "BeforeFile");
        if (before is not null)
        {
            model.BeforeImageUrl = before;
            ModelState.Remove(nameof(model.BeforeImageUrl));   // formda yeni yol görünsün
        }

        var after = await TryUploadAsync(afterFile, "AfterFile");
        if (after is not null)
        {
            model.AfterImageUrl = after;
            ModelState.Remove(nameof(model.AfterImageUrl));
        }

        // Dosya seçilmediyse (geçersiz dosya hatası zaten eklendi) ve mevcut fotoğraf da yoksa.
        if (string.IsNullOrWhiteSpace(model.BeforeImageUrl) && beforeFile is not { Length: > 0 })
            ModelState.AddModelError("BeforeFile", "Öncesi fotoğrafı zorunludur.");

        if (string.IsNullOrWhiteSpace(model.AfterImageUrl) && afterFile is not { Length: > 0 })
            ModelState.AddModelError("AfterFile", "Sonrası fotoğrafı zorunludur.");
    }

    private async Task<string?> TryUploadAsync(IFormFile? file, string errorKey)
    {
        if (file is not { Length: > 0 }) return null;

        var path = await ImageUploadHelper.SaveImageAsync(file, _environment.WebRootPath);
        if (path is null)
            ModelState.AddModelError(errorKey, InvalidImageText);
        return path;
    }
}
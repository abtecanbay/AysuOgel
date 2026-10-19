using System.Security.Cryptography;
using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ServicesController : Controller
{
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private readonly IServiceService _services;
    private readonly IWebHostEnvironment _environment;

    public ServicesController(IServiceService services, IWebHostEnvironment environment)
    {
        _services = services;
        _environment = environment;
    }

    public async Task<IActionResult> Index() =>
        View(await _services.GetAllAsync());

    
    public IActionResult Create() => View(new Service());

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxImageBytes + 256 * 1024)]
    public async Task<IActionResult> Create(
        [Bind("Name,ShortDescription,Description,ImageUrl,DisplayOrder,IsActive")] Service model,
        IFormFile? imageFile)
    {
        if (imageFile is { Length: > 0 })
        {
            var uploadedPath = await SaveImageAsync(imageFile);
            if (uploadedPath is null)
                ModelState.AddModelError("ImageFile", "Yalnızca geçerli JPG, PNG veya WEBP görselleri yükleyebilirsiniz. En fazla 5 MB.");
            else
                model.ImageUrl = uploadedPath;
        }

        if (!ModelState.IsValid) return View(model);

        await _services.CreateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _services.GetByIdAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxImageBytes + 256 * 1024)]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,ShortDescription,Description,ImageUrl,DisplayOrder,IsActive")] Service model,
        IFormFile? imageFile)
    {
        if (id != model.Id) return BadRequest();

        if (imageFile is { Length: > 0 })
        {
            var uploadedPath = await SaveImageAsync(imageFile);
            if (uploadedPath is null)
                ModelState.AddModelError("ImageFile", "Yalnızca geçerli JPG, PNG veya WEBP görselleri yükleyebilirsiniz. En fazla 5 MB.");
            else
                model.ImageUrl = uploadedPath;
        }

        if (!ModelState.IsValid) return View(model);

        await _services.UpdateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _services.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveImageAsync(IFormFile file)
    {
        if (file.Length <= 0 || file.Length > MaxImageBytes)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return null;

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
        if (!HasValidImageSignature(header, read, extension))
            return null;

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsPath, fileName);
        stream.Position = 0;
        await using (var output = System.IO.File.Create(fullPath))
        {
            await stream.CopyToAsync(output);
        }

        return $"/uploads/{fileName}";
    }

    private static bool HasValidImageSignature(byte[] bytes, int length, string extension)
    {
        if (extension is ".jpg" or ".jpeg")
            return length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

        if (extension == ".png")
            return length >= 8 &&
                   bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                   bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

        if (extension == ".webp")
            return length >= 12 &&
                   bytes[0] == (byte)'R' && bytes[1] == (byte)'I' &&
                   bytes[2] == (byte)'F' && bytes[3] == (byte)'F' &&
                   bytes[8] == (byte)'W' && bytes[9] == (byte)'E' &&
                   bytes[10] == (byte)'B' && bytes[11] == (byte)'P';

        return false;
    }
}
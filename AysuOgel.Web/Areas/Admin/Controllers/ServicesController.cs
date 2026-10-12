using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ServicesController : Controller
{
    private readonly IServiceService _services;
    public ServicesController(IServiceService services) => _services = services;

    public async Task<IActionResult> Index() =>
        View(await _services.GetAllAsync());

    public IActionResult Create() => View(new Service());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,ShortDescription,Description,ImageUrl,DisplayOrder,IsActive")] Service model)
    {
        if (!ModelState.IsValid) return View(model);
        await _services.CreateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _services.GetByIdAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,ShortDescription,Description,ImageUrl,DisplayOrder,IsActive")] Service model)
    {
        if (id != model.Id) return BadRequest();
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
}
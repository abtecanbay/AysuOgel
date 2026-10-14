using AysuOgel.Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MessagesController : Controller
{
    private readonly IContactService _contacts;
    public MessagesController(IContactService contacts) => _contacts = contacts;

    public async Task<IActionResult> Index() =>
        View(await _contacts.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var model = await _contacts.GetByIdAsync(id);
        if (model is null) return NotFound();

        await _contacts.MarkAsReadAsync(id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _contacts.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
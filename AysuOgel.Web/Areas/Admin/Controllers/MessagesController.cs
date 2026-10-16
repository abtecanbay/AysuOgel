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
public class MessagesController : Controller
{
    private readonly IContactService _contacts;
    private readonly IMessageReplyService _replies;

    public MessagesController(IContactService contacts, IMessageReplyService replies)
    {
        _contacts = contacts;
        _replies = replies;
    }

    public async Task<IActionResult> Index(MessageFilter filter)
    {
        ViewData["Filter"] = filter;
        return View(await _contacts.GetAllAsync(filter));
    }

    public async Task<IActionResult> Details(int id)
    {
        // Önce okundu işaretle ki ekranda güncel durum görünsün.
        await _contacts.MarkAsReadAsync(id);

        var vm = await BuildAsync(id, null);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, ReplyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var invalid = await BuildAsync(id, model);
            return invalid is null ? NotFound() : View("Details", invalid);
        }

        var reply = await _replies.ReplyAsync(id, model.Subject, model.Body);
        if (reply is null) return NotFound();

        var (text, level) = reply.DeliveryStatus switch
        {
            ReplyDeliveryStatus.Gonderildi =>
                ("Yanıt e-posta ile gönderildi.", "success"),
            ReplyDeliveryStatus.Hata =>
                ("Yanıt kaydedildi ancak e-posta gönderilemedi, mesaj \"Cevaplandı\" yapılmadı. Ayrıntı aşağıdaki kayıtta.", "danger"),
            _ =>
                ("Yanıt kaydedildi. E-posta altyapısı yapılandırılmadığı için e-posta gönderilmedi; yanıt \"Gönderim bekliyor\" olarak duruyor.", "warning")
        };
        TempData["ReplyMessage"] = text;
        TempData["ReplyLevel"] = level;

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _contacts.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<MessageDetailsViewModel?> BuildAsync(int id, ReplyFormViewModel? form)
    {
        var message = await _contacts.GetByIdAsync(id);
        if (message is null) return null;

        return new MessageDetailsViewModel
        {
            Message = message,
            Replies = await _replies.GetByMessageAsync(id),
            Reply = form ?? new ReplyFormViewModel { Subject = "Re: " + message.Subject.DisplayName() },
            EmailConfigured = _replies.IsEmailConfigured
        };
    }
}
using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class MessageDetailsViewModel
{
    public ContactMessage Message { get; set; } = null!;
    public List<MessageReply> Replies { get; set; } = new();
    public ReplyFormViewModel Reply { get; set; } = new();
    public bool EmailConfigured { get; set; }
}
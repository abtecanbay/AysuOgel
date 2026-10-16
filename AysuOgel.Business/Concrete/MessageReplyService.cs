using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AysuOgel.Business.Concrete;

public class MessageReplyService : IMessageReplyService
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;
    private readonly ILogger<MessageReplyService> _logger;

    public MessageReplyService(AppDbContext db, IEmailSender email, ILogger<MessageReplyService> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public bool IsEmailConfigured => _email.IsConfigured;

    public Task<List<MessageReply>> GetByMessageAsync(int messageId) =>
        _db.MessageReplies.AsNoTracking()
            .Where(r => r.ContactMessageId == messageId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

    public async Task<MessageReply?> ReplyAsync(int messageId, string subject, string body)
    {
        var message = await _db.ContactMessages.FindAsync(messageId);
        if (message is null) return null;

        var reply = new MessageReply
        {
            ContactMessageId = message.Id,
            ToEmail = message.Email,
            Subject = SingleLine(subject),
            Body = body.Trim(),
            DeliveryStatus = ReplyDeliveryStatus.Bekliyor,
            CreatedAt = DateTime.UtcNow
        };

        EmailSendResult result;
        try
        {
            result = await _email.SendAsync(reply.ToEmail, reply.Subject, reply.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yanıt e-postası gönderilemedi. MessageId={MessageId}", messageId);
            result = new EmailSendResult(EmailSendState.Failed, ex.Message);
        }

        switch (result.State)
        {
            case EmailSendState.Sent:
                reply.DeliveryStatus = ReplyDeliveryStatus.Gonderildi;
                reply.SentAt = DateTime.UtcNow;
                break;
            case EmailSendState.Failed:
                reply.DeliveryStatus = ReplyDeliveryStatus.Hata;
                reply.ErrorMessage = Truncate(result.Error, 500);
                break;
            default:
                reply.DeliveryStatus = ReplyDeliveryStatus.Bekliyor;   // e-posta altyapısı yok
                break;
        }

        message.IsRead = true;
        if (result.State != EmailSendState.Failed)
            message.RepliedAt = DateTime.UtcNow;

        _db.MessageReplies.Add(reply);
        await _db.SaveChangesAsync();
        return reply;
    }

    // E-posta başlığına satır sonu girmesin.
    private static string SingleLine(string text) =>
        string.Join(' ', text.Split(new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string? Truncate(string? text, int max) =>
        text is null ? null : text.Length <= max ? text : text[..max];
}
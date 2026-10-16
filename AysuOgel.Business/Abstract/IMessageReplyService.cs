using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IMessageReplyService
{
    bool IsEmailConfigured { get; }
    Task<List<MessageReply>> GetByMessageAsync(int messageId);

    /// <summary>Yanıtı kaydeder ve göndermeyi dener. Mesaj yoksa null döner.</summary>
    Task<MessageReply?> ReplyAsync(int messageId, string subject, string body);
}
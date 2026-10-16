namespace AysuOgel.Business.Abstract;

public enum EmailSendState { Sent, NotConfigured, Failed }

public record EmailSendResult(EmailSendState State, string? Error = null);

public interface IEmailSender
{
    /// <summary>E-posta altyapısı kurulu mu?</summary>
    bool IsConfigured { get; }

    Task<EmailSendResult> SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
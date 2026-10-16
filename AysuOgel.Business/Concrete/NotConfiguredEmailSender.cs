using AysuOgel.Business.Abstract;

namespace AysuOgel.Business.Concrete;

// SMTP gelene kadar kullanılır: hiçbir şey göndermez, "yapılandırılmadı" der.
public class NotConfiguredEmailSender : IEmailSender
{
    public bool IsConfigured => false;

    public Task<EmailSendResult> SendAsync(string to, string subject, string body, CancellationToken ct = default) =>
        Task.FromResult(new EmailSendResult(EmailSendState.NotConfigured));
}
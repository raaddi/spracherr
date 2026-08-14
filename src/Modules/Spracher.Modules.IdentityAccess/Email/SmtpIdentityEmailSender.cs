using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Spracher.Modules.IdentityAccess.Email;

internal sealed class SmtpIdentityEmailSender(IOptions<SmtpOptions> options)
    : IIdentityEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(
        IdentityEmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var mimeMessage = new MimeMessage
        {
            Subject = message.Subject,
            Body = new BodyBuilder
            {
                TextBody = message.PlainTextBody,
                HtmlBody = message.HtmlBody,
            }.ToMessageBody(),
        };
        mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.Recipient));

        using var smtpClient = new SmtpClient();
        var socketOptions = _options.UseSslOnConnect
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await smtpClient.ConnectAsync(
            _options.Host,
            _options.Port,
            socketOptions,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await smtpClient.AuthenticateAsync(
                _options.UserName,
                _options.Password,
                cancellationToken);
        }

        await smtpClient.SendAsync(mimeMessage, cancellationToken);
        await smtpClient.DisconnectAsync(quit: true, cancellationToken);
    }
}

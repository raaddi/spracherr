namespace Spracher.Modules.IdentityAccess.Email;

internal interface IIdentityEmailSender
{
    Task SendAsync(
        IdentityEmailMessage message,
        CancellationToken cancellationToken = default);
}

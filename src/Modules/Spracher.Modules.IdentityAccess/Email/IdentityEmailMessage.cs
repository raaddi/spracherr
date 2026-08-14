namespace Spracher.Modules.IdentityAccess.Email;

internal sealed record IdentityEmailMessage(
    string Recipient,
    string Subject,
    string PlainTextBody,
    string HtmlBody,
    string ActionUrl,
    DateTimeOffset CreatedAt);

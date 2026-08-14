using System.Collections.Concurrent;

namespace Spracher.Modules.IdentityAccess.Email;

internal interface IDevelopmentEmailStore
{
    void Store(IdentityEmailMessage message);

    IdentityEmailMessage? GetLatest(string recipient);
}

internal sealed class DevelopmentEmailStore : IDevelopmentEmailStore
{
    private readonly ConcurrentDictionary<string, IdentityEmailMessage> _messages =
        new(StringComparer.OrdinalIgnoreCase);

    public void Store(IdentityEmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _messages[message.Recipient.Trim()] = message;
    }

    public IdentityEmailMessage? GetLatest(string recipient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        return _messages.GetValueOrDefault(recipient.Trim());
    }
}

internal sealed class DevelopmentIdentityEmailSender(IDevelopmentEmailStore store)
    : IIdentityEmailSender
{
    public Task SendAsync(
        IdentityEmailMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.Store(message);
        return Task.CompletedTask;
    }
}

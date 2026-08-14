namespace Spracher.Contracts.Identity;

public sealed record DevelopmentEmailResponse(
    string Recipient,
    string Subject,
    string ActionUrl,
    DateTimeOffset CreatedAt);

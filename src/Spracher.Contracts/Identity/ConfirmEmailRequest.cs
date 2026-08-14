namespace Spracher.Contracts.Identity;

public sealed record ConfirmEmailRequest(Guid UserId, string Code);

namespace Spracher.Contracts.Languages;

public sealed record UserLanguageProfileResponse(
    Guid LanguageId,
    string Code,
    string Name,
    string NativeName,
    bool IsNative,
    bool IsLearning,
    string? CurrentCefrLevel,
    DateTimeOffset? StartedAt);

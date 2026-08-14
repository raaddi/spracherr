namespace Spracher.Contracts.Languages;

public sealed record UpdateUserLanguagesRequest(
    IReadOnlyList<UserLanguageSelectionRequest> Languages);

public sealed record UserLanguageSelectionRequest(
    Guid LanguageId,
    bool IsNative,
    bool IsLearning);

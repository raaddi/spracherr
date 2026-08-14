namespace Spracher.Contracts.Languages;

public sealed record LanguageResponse(
    Guid Id,
    string Code,
    string Name,
    string NativeName,
    string TextDirection);

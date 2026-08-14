using Spracher.Modules.Languages.Domain;

namespace Spracher.Modules.Languages.Infrastructure;

internal static class LanguageSeedData
{
    public static IReadOnlyList<Language> All { get; } =
    [
        new(
            Guid.Parse("0198ac50-0000-7000-8000-000000000001"),
            "pl",
            "Polish",
            "Polski",
            TextDirection.LeftToRight,
            isActive: true),
        new(
            Guid.Parse("0198ac50-0000-7000-8000-000000000002"),
            "en",
            "English",
            "English",
            TextDirection.LeftToRight,
            isActive: true),
        new(
            Guid.Parse("0198ac50-0000-7000-8000-000000000003"),
            "de",
            "German",
            "Deutsch",
            TextDirection.LeftToRight,
            isActive: true),
        new(
            Guid.Parse("0198ac50-0000-7000-8000-000000000004"),
            "es",
            "Spanish",
            "Español",
            TextDirection.LeftToRight,
            isActive: true),
        new(
            Guid.Parse("0198ac50-0000-7000-8000-000000000005"),
            "fr",
            "French",
            "Français",
            TextDirection.LeftToRight,
            isActive: true),
    ];
}

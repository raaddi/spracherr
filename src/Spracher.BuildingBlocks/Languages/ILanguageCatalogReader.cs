namespace Spracher.BuildingBlocks.Languages;

public interface ILanguageCatalogReader
{
    Task<IReadOnlyDictionary<Guid, LanguageDescriptor>> GetActiveByIdsAsync(
        IReadOnlyCollection<Guid> languageIds,
        CancellationToken cancellationToken = default);
}

public sealed record LanguageDescriptor(
    Guid Id,
    string Code,
    string Name,
    string NativeName);

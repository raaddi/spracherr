using Microsoft.EntityFrameworkCore;
using Spracher.BuildingBlocks.Languages;
using Spracher.Modules.Languages.Domain;
using Spracher.Persistence;

namespace Spracher.Modules.Languages.Application;

internal sealed class LanguageCatalogReader(SpracherDbContext dbContext)
    : ILanguageCatalogReader
{
    public async Task<IReadOnlyDictionary<Guid, LanguageDescriptor>> GetActiveByIdsAsync(
        IReadOnlyCollection<Guid> languageIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(languageIds);

        if (languageIds.Count == 0)
        {
            return new Dictionary<Guid, LanguageDescriptor>();
        }

        return await dbContext.Set<Language>()
            .AsNoTracking()
            .Where(language => language.IsActive && languageIds.Contains(language.Id))
            .Select(language => new LanguageDescriptor(
                language.Id,
                language.Code,
                language.Name,
                language.NativeName))
            .ToDictionaryAsync(language => language.Id, cancellationToken);
    }
}

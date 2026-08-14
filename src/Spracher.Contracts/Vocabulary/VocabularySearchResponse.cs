namespace Spracher.Contracts.Vocabulary;

public sealed record VocabularySearchResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<VocabularySearchItemResponse> Items);

public sealed record VocabularySearchItemResponse(
    Guid LexemeId,
    string Lemma,
    Guid LanguageId,
    string LanguageCode,
    string LanguageName,
    string PartOfSpeech,
    string? CefrLevel,
    int? FrequencyRank,
    bool IsPrivate,
    int SenseCount);

namespace Spracher.Contracts.Vocabulary;

public sealed record VocabularyListsResponse(
    IReadOnlyList<VocabularyListSummaryResponse> Items);

public sealed record VocabularyListSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    int ItemCount,
    DateTimeOffset UpdatedAt);

public sealed record VocabularyListDetailsResponse(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<VocabularyListItemResponse> Items,
    DateTimeOffset UpdatedAt);

public sealed record VocabularyListItemResponse(
    Guid UserVocabularyItemId,
    Guid LexemeSenseId,
    Guid LexemeId,
    string Lemma,
    string LanguageCode,
    string Status,
    int Position,
    string? Note,
    DateTimeOffset AddedAt);

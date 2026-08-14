namespace Spracher.Contracts.Vocabulary;

public sealed record UserVocabularyResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<UserVocabularyItemResponse> Items);

public sealed record UserVocabularyItemResponse(
    Guid Id,
    Guid LexemeSenseId,
    Guid LexemeId,
    string Lemma,
    Guid LanguageId,
    string LanguageCode,
    string LanguageName,
    string PartOfSpeech,
    string Definition,
    string Status,
    bool IsPrivate,
    DateTimeOffset AddedAt,
    DateTimeOffset StatusChangedAt);

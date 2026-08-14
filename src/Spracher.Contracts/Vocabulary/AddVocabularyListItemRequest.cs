namespace Spracher.Contracts.Vocabulary;

public sealed record AddVocabularyListItemRequest(
    Guid UserVocabularyItemId,
    string? Note);

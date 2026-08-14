namespace Spracher.Contracts.Vocabulary;

public sealed record CreatePrivateVocabularyRequest(
    Guid LanguageId,
    string Lemma,
    string PartOfSpeech,
    string? CefrLevel,
    string? Notes,
    Guid DefinitionLanguageId,
    string Definition);

namespace Spracher.Contracts.Vocabulary;

public sealed record VocabularyDetailsResponse(
    Guid LexemeId,
    string Lemma,
    Guid LanguageId,
    string LanguageCode,
    string LanguageName,
    string PartOfSpeech,
    string? CefrLevel,
    int? FrequencyRank,
    string? Notes,
    bool IsPrivate,
    string SourceType,
    string? SourceReference,
    IReadOnlyList<WordFormResponse> WordForms,
    IReadOnlyList<PronunciationResponse> Pronunciations,
    IReadOnlyList<LexemeFeatureResponse> Features,
    IReadOnlyList<LexemeSenseResponse> Senses);

public sealed record WordFormResponse(string Form, string GrammarTags);

public sealed record PronunciationResponse(
    string Scheme,
    string Value,
    string? Region,
    string? AudioAssetReference);

public sealed record LexemeFeatureResponse(string Key, string Value);

public sealed record LexemeSenseResponse(
    Guid SenseId,
    Guid ConceptId,
    string Definition,
    string DefinitionLanguageCode,
    string? Register,
    string? CefrLevel,
    IReadOnlyList<EquivalentLexemeResponse> Equivalents,
    IReadOnlyList<ExampleSentenceResponse> Examples);

public sealed record EquivalentLexemeResponse(
    Guid LexemeId,
    Guid SenseId,
    string Lemma,
    string LanguageCode,
    string PartOfSpeech);

public sealed record ExampleSentenceResponse(
    Guid Id,
    string Text,
    string LanguageCode,
    int? HighlightStart,
    int? HighlightLength);

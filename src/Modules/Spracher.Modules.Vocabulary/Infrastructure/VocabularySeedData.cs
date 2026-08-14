using Spracher.BuildingBlocks.Languages;
using Spracher.Modules.Vocabulary.Domain;

namespace Spracher.Modules.Vocabulary.Infrastructure;

internal static class VocabularySeedData
{
    public static readonly Guid PolishLanguageId =
        Guid.Parse("0198ac50-0000-7000-8000-000000000001");

    public static readonly Guid EnglishLanguageId =
        Guid.Parse("0198ac50-0000-7000-8000-000000000002");

    private static readonly DateTimeOffset SeededAt =
        new(2026, 8, 14, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<Concept> Concepts { get; } =
    [
        CatalogConcept("0198ae00-0000-7000-8000-000000000001", "bank.financial-institution"),
        CatalogConcept("0198ae00-0000-7000-8000-000000000002", "bank.river-edge"),
        CatalogConcept("0198ae00-0000-7000-8000-000000000003", "run.move-quickly"),
        CatalogConcept("0198ae00-0000-7000-8000-000000000004", "apple.fruit"),
    ];

    public static IReadOnlyList<Lexeme> Lexemes { get; } =
    [
        CatalogLexeme("0198ae10-0000-7000-8000-000000000001", EnglishLanguageId, "bank", PartOfSpeech.Noun, CefrLevel.A2, 520),
        CatalogLexeme("0198ae10-0000-7000-8000-000000000002", PolishLanguageId, "bank", PartOfSpeech.Noun, CefrLevel.A2, 780),
        CatalogLexeme("0198ae10-0000-7000-8000-000000000003", PolishLanguageId, "brzeg", PartOfSpeech.Noun, CefrLevel.A2, 690),
        CatalogLexeme("0198ae10-0000-7000-8000-000000000004", EnglishLanguageId, "run", PartOfSpeech.Verb, CefrLevel.A1, 180),
        CatalogLexeme("0198ae10-0000-7000-8000-000000000005", PolishLanguageId, "biec", PartOfSpeech.Verb, CefrLevel.A1, 410),
        CatalogLexeme("0198ae10-0000-7000-8000-000000000006", EnglishLanguageId, "apple", PartOfSpeech.Noun, CefrLevel.A1, 1120),
        CatalogLexeme("0198ae10-0000-7000-8000-000000000007", PolishLanguageId, "jabłko", PartOfSpeech.Noun, CefrLevel.A1, 990),
    ];

    public static IReadOnlyList<LexemeSense> Senses { get; } =
    [
        CatalogSense("0198ae20-0000-7000-8000-000000000001", "0198ae10-0000-7000-8000-000000000001", "0198ae00-0000-7000-8000-000000000001", EnglishLanguageId, "An organization that keeps, lends, and exchanges money."),
        CatalogSense("0198ae20-0000-7000-8000-000000000002", "0198ae10-0000-7000-8000-000000000001", "0198ae00-0000-7000-8000-000000000002", EnglishLanguageId, "The land along the edge of a river."),
        CatalogSense("0198ae20-0000-7000-8000-000000000003", "0198ae10-0000-7000-8000-000000000002", "0198ae00-0000-7000-8000-000000000001", PolishLanguageId, "Instytucja przechowująca pieniądze i udzielająca pożyczek."),
        CatalogSense("0198ae20-0000-7000-8000-000000000004", "0198ae10-0000-7000-8000-000000000003", "0198ae00-0000-7000-8000-000000000002", PolishLanguageId, "Pas lądu znajdujący się przy rzece."),
        CatalogSense("0198ae20-0000-7000-8000-000000000005", "0198ae10-0000-7000-8000-000000000004", "0198ae00-0000-7000-8000-000000000003", EnglishLanguageId, "To move quickly on foot."),
        CatalogSense("0198ae20-0000-7000-8000-000000000006", "0198ae10-0000-7000-8000-000000000005", "0198ae00-0000-7000-8000-000000000003", PolishLanguageId, "Poruszać się szybko, odbijając się stopami od podłoża."),
        CatalogSense("0198ae20-0000-7000-8000-000000000007", "0198ae10-0000-7000-8000-000000000006", "0198ae00-0000-7000-8000-000000000004", EnglishLanguageId, "A round fruit with firm flesh and thin skin."),
        CatalogSense("0198ae20-0000-7000-8000-000000000008", "0198ae10-0000-7000-8000-000000000007", "0198ae00-0000-7000-8000-000000000004", PolishLanguageId, "Okrągły owoc jabłoni o cienkiej skórce."),
    ];

    public static IReadOnlyList<WordForm> WordForms { get; } =
    [
        new(Guid.Parse("0198ae30-0000-7000-8000-000000000001"), LexemeId(1), "banks", "plural"),
        new(Guid.Parse("0198ae30-0000-7000-8000-000000000002"), LexemeId(4), "ran", "past"),
        new(Guid.Parse("0198ae30-0000-7000-8000-000000000003"), LexemeId(4), "running", "present-participle"),
        new(Guid.Parse("0198ae30-0000-7000-8000-000000000004"), LexemeId(6), "apples", "plural"),
        new(Guid.Parse("0198ae30-0000-7000-8000-000000000005"), LexemeId(7), "jabłka", "genitive-singular;nominative-plural"),
    ];

    public static IReadOnlyList<Pronunciation> Pronunciations { get; } =
    [
        new(Guid.Parse("0198ae40-0000-7000-8000-000000000001"), LexemeId(1), "IPA", "/bæŋk/", "en", null),
        new(Guid.Parse("0198ae40-0000-7000-8000-000000000002"), LexemeId(4), "IPA", "/rʌn/", "en", null),
        new(Guid.Parse("0198ae40-0000-7000-8000-000000000003"), LexemeId(6), "IPA", "/ˈæp.əl/", "en", null),
        new(Guid.Parse("0198ae40-0000-7000-8000-000000000004"), LexemeId(7), "IPA", "/ˈjap.kɔ/", "pl", null),
    ];

    public static IReadOnlyList<LexemeFeature> Features { get; } =
    [
        new(Guid.Parse("0198ae50-0000-7000-8000-000000000001"), LexemeId(2), "gender", "masculine"),
        new(Guid.Parse("0198ae50-0000-7000-8000-000000000002"), LexemeId(3), "gender", "masculine"),
        new(Guid.Parse("0198ae50-0000-7000-8000-000000000003"), LexemeId(7), "gender", "neuter"),
    ];

    public static IReadOnlyList<ExampleSentence> Examples { get; } =
    [
        CatalogExample("0198ae60-0000-7000-8000-000000000001", EnglishLanguageId, "She works at a bank in the city centre."),
        CatalogExample("0198ae60-0000-7000-8000-000000000002", EnglishLanguageId, "We sat on the river bank."),
        CatalogExample("0198ae60-0000-7000-8000-000000000003", PolishLanguageId, "Usiedliśmy na brzegu rzeki."),
        CatalogExample("0198ae60-0000-7000-8000-000000000004", EnglishLanguageId, "I run every morning."),
        CatalogExample("0198ae60-0000-7000-8000-000000000005", PolishLanguageId, "Lubię biec rano."),
        CatalogExample("0198ae60-0000-7000-8000-000000000006", EnglishLanguageId, "This apple is sweet."),
    ];

    public static IReadOnlyList<ExampleUsage> ExampleUsages { get; } =
    [
        new(SenseId(1), ExampleId(1), 15, 4),
        new(SenseId(2), ExampleId(2), 20, 4),
        new(SenseId(4), ExampleId(3), 14, 6),
        new(SenseId(5), ExampleId(4), 2, 3),
        new(SenseId(6), ExampleId(5), 5, 4),
        new(SenseId(7), ExampleId(6), 5, 5),
    ];

    private static Concept CatalogConcept(string id, string key) =>
        new(
            Guid.Parse(id),
            key,
            VocabularyVisibility.Catalog,
            VocabularySourceType.Curated,
            "spracher-curated-en-pl-v1",
            PublicationStatus.Published,
            ownerUserId: null,
            SeededAt);

    private static Lexeme CatalogLexeme(
        string id,
        Guid languageId,
        string lemma,
        PartOfSpeech partOfSpeech,
        CefrLevel cefrLevel,
        int frequencyRank) =>
        new(
            Guid.Parse(id),
            languageId,
            lemma,
            partOfSpeech,
            cefrLevel,
            frequencyRank,
            notes: null,
            VocabularyVisibility.Catalog,
            VocabularySourceType.Curated,
            "spracher-curated-en-pl-v1",
            PublicationStatus.Published,
            ownerUserId: null,
            SeededAt);

    private static LexemeSense CatalogSense(
        string id,
        string lexemeId,
        string conceptId,
        Guid definitionLanguageId,
        string definition) =>
        new(
            Guid.Parse(id),
            Guid.Parse(lexemeId),
            Guid.Parse(conceptId),
            definitionLanguageId,
            definition,
            register: null,
            cefrLevelOverride: null,
            VocabularyVisibility.Catalog,
            PublicationStatus.Published,
            ownerUserId: null,
            SeededAt);

    private static ExampleSentence CatalogExample(string id, Guid languageId, string text) =>
        new(
            Guid.Parse(id),
            languageId,
            text,
            "spracher-curated-en-pl-v1",
            VocabularyVisibility.Catalog,
            PublicationStatus.Published,
            ownerUserId: null,
            SeededAt);

    private static Guid LexemeId(int suffix) =>
        Guid.Parse($"0198ae10-0000-7000-8000-{suffix:D12}");

    private static Guid SenseId(int suffix) =>
        Guid.Parse($"0198ae20-0000-7000-8000-{suffix:D12}");

    private static Guid ExampleId(int suffix) =>
        Guid.Parse($"0198ae60-0000-7000-8000-{suffix:D12}");
}

using Spracher.Modules.Exercises.Domain;
using Spracher.Modules.Exercises.Engine;

namespace Spracher.Modules.Exercises.Infrastructure;

internal static class ExerciseSeedData
{
    private static readonly DateTimeOffset SeededAt =
        new(2026, 8, 14, 0, 0, 0, TimeSpan.Zero);

    public static readonly Guid PresentSimpleDefinitionId =
        Guid.Parse("0198b100-0000-7000-8000-000000000001");

    public static readonly Guid PresentSimpleVersionId =
        Guid.Parse("0198b110-0000-7000-8000-000000000001");

    public static readonly Guid FillInBlankDefinitionId =
        Guid.Parse("0198b100-0000-7000-8000-000000000002");

    public static readonly Guid FillInBlankVersionId =
        Guid.Parse("0198b110-0000-7000-8000-000000000002");

    public static readonly Guid TranslationDefinitionId =
        Guid.Parse("0198b100-0000-7000-8000-000000000003");

    public static readonly Guid TranslationVersionId =
        Guid.Parse("0198b110-0000-7000-8000-000000000003");

    public static readonly Guid PresentSimpleSetId =
        Guid.Parse("0198b120-0000-7000-8000-000000000001");

    public static IReadOnlyList<ExerciseDefinition> Definitions { get; } =
    [
        new(
            PresentSimpleDefinitionId,
            ExerciseTypeKeys.MultipleChoice,
            "Present Simple: third person",
            "Choose the correct verb form for he, she or it.",
            ownerUserId: null,
            SeededAt),
        new(
            FillInBlankDefinitionId,
            ExerciseTypeKeys.FillInBlank,
            "Present Simple: missing verb",
            "Complete the sentence with the correct third-person verb form.",
            ownerUserId: null,
            SeededAt),
        new(
            TranslationDefinitionId,
            ExerciseTypeKeys.Translation,
            "Present Simple: translate a sentence",
            "Translate a short Present Simple sentence into Polish.",
            ownerUserId: null,
            SeededAt),
    ];

    public static IReadOnlyList<ExerciseVersion> Versions { get; } =
    [
        new(
            PresentSimpleVersionId,
            PresentSimpleDefinitionId,
            versionNumber: 1,
            schemaVersion: 1,
            "Choose the correct sentence.",
            """
            {
              "options": [
                { "id": "work", "text": "She work in a bank." },
                { "id": "works", "text": "She works in a bank." },
                { "id": "working", "text": "She working in a bank." }
              ],
              "correctOptionIds": ["works"],
              "points": 10,
              "correctFeedback": "Exactly — use -s with she in the Present Simple.",
              "incorrectFeedback": "Remember: in the Present Simple, he/she/it takes -s."
            }
            """,
            ExerciseVersionStatus.Published,
            SeededAt,
            SeededAt),
        new(
            FillInBlankVersionId,
            FillInBlankDefinitionId,
            versionNumber: 1,
            schemaVersion: 1,
            "Complete the missing word.",
            """
            {
              "segments": [
                { "kind": "text", "text": "She ", "blankId": null },
                { "kind": "blank", "text": null, "blankId": "verb" },
                { "kind": "text", "text": " to school every day.", "blankId": null }
              ],
              "answers": { "verb": ["goes"] },
              "caseSensitive": false,
              "trimWhitespace": true,
              "points": 10,
              "correctFeedback": "Correct — go changes to goes with she.",
              "incorrectFeedback": "Use the third-person singular form: goes."
            }
            """,
            ExerciseVersionStatus.Published,
            SeededAt,
            SeededAt),
        new(
            TranslationVersionId,
            TranslationDefinitionId,
            versionNumber: 1,
            schemaVersion: 1,
            "Translate the sentence into Polish.",
            """
            {
              "sourceText": "She goes to school every day.",
              "sourceLanguageCode": "en",
              "targetLanguageCode": "pl",
              "acceptedAnswers": [
                "Ona chodzi do szkoły codziennie.",
                "Ona codziennie chodzi do szkoły."
              ],
              "caseSensitive": false,
              "trimWhitespace": true,
              "collapseWhitespace": true,
              "ignoreTerminalPunctuation": true,
              "points": 10,
              "correctFeedback": "Correct — both natural word orders are accepted.",
              "incorrectFeedback": "Check the verb form and the phrase every day."
            }
            """,
            ExerciseVersionStatus.Published,
            SeededAt,
            SeededAt),
    ];

    public static IReadOnlyList<ExerciseSet> Sets { get; } =
    [
        new(
            PresentSimpleSetId,
            "Present Simple: quick practice",
            "Three short exercises covering the third-person singular.",
            ownerUserId: null,
            ExerciseSetStatus.Published,
            SeededAt,
            SeededAt),
    ];

    public static IReadOnlyList<ExerciseSetItem> SetItems { get; } =
    [
        new(
            Guid.Parse("0198b130-0000-7000-8000-000000000001"),
            PresentSimpleSetId,
            PresentSimpleVersionId,
            position: 1),
        new(
            Guid.Parse("0198b130-0000-7000-8000-000000000002"),
            PresentSimpleSetId,
            FillInBlankVersionId,
            position: 2),
        new(
            Guid.Parse("0198b130-0000-7000-8000-000000000003"),
            PresentSimpleSetId,
            TranslationVersionId,
            position: 3),
    ];
}

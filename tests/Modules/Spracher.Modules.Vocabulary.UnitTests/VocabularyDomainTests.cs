using Spracher.BuildingBlocks.Languages;
using Spracher.Modules.Vocabulary.Domain;

namespace Spracher.Modules.Vocabulary.UnitTests;

public sealed class VocabularyDomainTests
{
    private static readonly Guid UserId =
        Guid.Parse("0198ae00-0000-7000-8000-000000000001");
    private static readonly Guid LanguageId =
        Guid.Parse("0198ae00-0000-7000-8000-000000000002");
    private static readonly DateTimeOffset Now =
        new(2026, 8, 14, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PrivateLexemeShouldRetainOwnershipAndNormalizedLemma()
    {
        var lexeme = Lexeme.CreatePrivate(
            UserId,
            LanguageId,
            "  River   Bank ",
            PartOfSpeech.Noun,
            CefrLevel.B1,
            "  Personal note  ",
            Now);

        Assert.Equal(UserId, lexeme.OwnerUserId);
        Assert.Equal(VocabularyVisibility.Private, lexeme.Visibility);
        Assert.Equal(VocabularySourceType.UserCreated, lexeme.SourceType);
        Assert.Equal(PublicationStatus.Draft, lexeme.PublicationStatus);
        Assert.Equal("River   Bank", lexeme.Lemma);
        Assert.Equal("river bank", lexeme.NormalizedLemma);
        Assert.Equal("Personal note", lexeme.Notes);
    }

    [Fact]
    public void OneLexemeCanRepresentMultipleSenses()
    {
        var lexeme = Lexeme.CreatePrivate(
            UserId,
            LanguageId,
            "bank",
            PartOfSpeech.Noun,
            CefrLevel.A2,
            notes: null,
            Now);
        var financialConcept = Concept.CreatePrivate(UserId, "private.financial-bank", Now);
        var riverConcept = Concept.CreatePrivate(UserId, "private.river-bank", Now);

        var financialSense = LexemeSense.CreatePrivate(
            UserId,
            lexeme.Id,
            financialConcept.Id,
            LanguageId,
            "financial institution",
            CefrLevel.A2,
            Now);
        var riverSense = LexemeSense.CreatePrivate(
            UserId,
            lexeme.Id,
            riverConcept.Id,
            LanguageId,
            "land next to a river",
            CefrLevel.B1,
            Now);

        Assert.Equal(financialSense.LexemeId, riverSense.LexemeId);
        Assert.NotEqual(financialSense.ConceptId, riverSense.ConceptId);
    }

    [Fact]
    public void UserVocabularyItemShouldTrackStatusChange()
    {
        var item = UserVocabularyItem.Create(UserId, Guid.NewGuid(), Now);
        var changedAt = Now.AddMinutes(5);

        item.ChangeStatus(UserVocabularyStatus.Learned, changedAt);

        Assert.Equal(UserVocabularyStatus.Learned, item.Status);
        Assert.Equal(changedAt, item.StatusChangedAt);
        Assert.Equal(Now, item.AddedAt);
    }

    [Fact]
    public void VocabularyListShouldNormalizeNameAndRetainOwner()
    {
        var list = VocabularyList.Create(
            UserId,
            "  Travel   Words ",
            "  Useful at the airport.  ",
            Now);

        Assert.Equal(UserId, list.OwnerUserId);
        Assert.Equal("Travel   Words", list.Name);
        Assert.Equal("travel words", list.NormalizedName);
        Assert.Equal("Useful at the airport.", list.Description);
    }

    [Fact]
    public void VocabularyCategoryShouldValidateAndNormalizeColor()
    {
        var category = VocabularyCategory.Create(UserId, "Difficult", "#2f8f63", Now);

        Assert.Equal("#2F8F63", category.Color);
        Assert.Throws<ArgumentException>(() =>
            VocabularyCategory.Create(UserId, "Invalid", "green", Now));
    }

    [Fact]
    public void VocabularyListItemShouldRejectNegativePosition()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VocabularyListItem.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                -1,
                null,
                Now));
    }
}

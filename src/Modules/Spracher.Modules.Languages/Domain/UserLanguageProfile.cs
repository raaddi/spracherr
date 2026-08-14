using Spracher.BuildingBlocks.Languages;

namespace Spracher.Modules.Languages.Domain;

public sealed class UserLanguageProfile
{
    private UserLanguageProfile()
    {
    }

    private UserLanguageProfile(
        Guid userId,
        Guid languageId,
        bool isNative,
        bool isLearning,
        DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        LanguageId = languageId;
        CreatedAt = createdAt;
        ApplySelection(isNative, isLearning, createdAt);
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid LanguageId { get; private set; }

    public bool IsNative { get; private set; }

    public bool IsLearning { get; private set; }

    public CefrLevel? CurrentCefrLevel { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static UserLanguageProfile Create(
        Guid userId,
        Guid languageId,
        bool isNative,
        bool isLearning,
        DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (languageId == Guid.Empty)
        {
            throw new ArgumentException("Language ID cannot be empty.", nameof(languageId));
        }

        EnsureSelected(isNative, isLearning);
        return new UserLanguageProfile(userId, languageId, isNative, isLearning, createdAt);
    }

    public void UpdateSelection(
        bool isNative,
        bool isLearning,
        DateTimeOffset updatedAt)
    {
        EnsureSelected(isNative, isLearning);
        ApplySelection(isNative, isLearning, updatedAt);
        UpdatedAt = updatedAt;
    }

    private static void EnsureSelected(bool isNative, bool isLearning)
    {
        if (!isNative && !isLearning)
        {
            throw new ArgumentException(
                "A language profile must be native, learning, or both.");
        }
    }

    private void ApplySelection(
        bool isNative,
        bool isLearning,
        DateTimeOffset changedAt)
    {
        if (isLearning && !IsLearning)
        {
            CurrentCefrLevel = CefrLevel.A0;
            StartedAt = changedAt;
        }
        else if (!isLearning)
        {
            CurrentCefrLevel = null;
            StartedAt = null;
        }

        IsNative = isNative;
        IsLearning = isLearning;
    }
}

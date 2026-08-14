namespace Spracher.Modules.Vocabulary.Domain;

public sealed class VocabularyList
{
    private VocabularyList()
    {
    }

    private VocabularyList(
        Guid ownerUserId,
        string name,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        NormalizedName = VocabularyTextNormalizer.NormalizeLemma(name);
        Description = description?.Trim();
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static VocabularyList Create(
        Guid ownerUserId,
        string name,
        string? description,
        DateTimeOffset createdAt)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Owner user ID cannot be empty.", nameof(ownerUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Trim().Length > 100)
        {
            throw new ArgumentException("List name cannot exceed 100 characters.", nameof(name));
        }

        if (description?.Length > 500)
        {
            throw new ArgumentException(
                "List description cannot exceed 500 characters.",
                nameof(description));
        }

        return new VocabularyList(ownerUserId, name, description, createdAt);
    }

    public void Touch(DateTimeOffset updatedAt)
    {
        if (updatedAt > UpdatedAt)
        {
            UpdatedAt = updatedAt;
        }
    }
}

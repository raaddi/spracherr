namespace Spracher.Modules.Vocabulary.Domain;

public sealed class VocabularyCategory
{
    private VocabularyCategory()
    {
    }

    private VocabularyCategory(
        Guid ownerUserId,
        string name,
        string color,
        DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        NormalizedName = VocabularyTextNormalizer.NormalizeLemma(name);
        Color = color;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string Color { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static VocabularyCategory Create(
        Guid ownerUserId,
        string name,
        string color,
        DateTimeOffset createdAt)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Owner user ID cannot be empty.", nameof(ownerUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Trim().Length > 60)
        {
            throw new ArgumentException("Category name cannot exceed 60 characters.", nameof(name));
        }

        if (!IsValidColor(color))
        {
            throw new ArgumentException("Category color must use #RRGGBB format.", nameof(color));
        }

        return new VocabularyCategory(ownerUserId, name, color.ToUpperInvariant(), createdAt);
    }

    public static bool IsValidColor(string? color)
    {
        if (color is not { Length: 7 } || color[0] != '#')
        {
            return false;
        }

        for (var index = 1; index < color.Length; index++)
        {
            if (!Uri.IsHexDigit(color[index]))
            {
                return false;
            }
        }

        return true;
    }
}

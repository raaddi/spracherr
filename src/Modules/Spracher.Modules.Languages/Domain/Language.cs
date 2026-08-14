namespace Spracher.Modules.Languages.Domain;

public sealed class Language
{
    private Language()
    {
    }

    internal Language(
        Guid id,
        string code,
        string name,
        string nativeName,
        TextDirection textDirection,
        bool isActive)
    {
        Id = id;
        Code = code;
        Name = name;
        NativeName = nativeName;
        TextDirection = textDirection;
        IsActive = isActive;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string NativeName { get; private set; } = string.Empty;

    public TextDirection TextDirection { get; private set; }

    public bool IsActive { get; private set; }
}

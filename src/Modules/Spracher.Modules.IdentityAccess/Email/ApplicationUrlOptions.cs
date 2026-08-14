namespace Spracher.Modules.IdentityAccess.Email;

internal sealed class ApplicationUrlOptions
{
    public const string SectionName = "Application";

    public string PublicUrl { get; init; } = string.Empty;
}

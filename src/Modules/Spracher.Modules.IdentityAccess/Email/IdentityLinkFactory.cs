using Microsoft.Extensions.Options;

namespace Spracher.Modules.IdentityAccess.Email;

internal sealed class IdentityLinkFactory(IOptions<ApplicationUrlOptions> options)
{
    private readonly Uri _publicBaseUri = CreateBaseUri(options.Value.PublicUrl);

    public Uri CreateEmailConfirmationUrl(Guid userId, string code) =>
        CreateUrl(
            "confirm-email",
            ("userId", userId.ToString()),
            ("code", code));

    public Uri CreatePasswordResetUrl(string email, string code) =>
        CreateUrl(
            "reset-password",
            ("email", email),
            ("code", code));

    private static Uri CreateBaseUri(string publicUrl)
    {
        var normalizedUrl = publicUrl.EndsWith('/')
            ? publicUrl
            : $"{publicUrl}/";

        return new Uri(normalizedUrl, UriKind.Absolute);
    }

    private Uri CreateUrl(string relativePath, params (string Key, string Value)[] query)
    {
        var uriBuilder = new UriBuilder(new Uri(_publicBaseUri, relativePath))
        {
            Query = string.Join(
                "&",
                query.Select(item =>
                    $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}")),
        };

        return uriBuilder.Uri;
    }
}

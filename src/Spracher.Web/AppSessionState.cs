using Spracher.Contracts.Identity;
using Spracher.Web.ApiClient;

namespace Spracher.Web;

public sealed class AppSessionState(AuthApiClient authApiClient)
{
    private bool _initialized;

    public event Action? Changed;

    public AuthSessionResponse Session { get; private set; } = new(false, null);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await RefreshAsync(cancellationToken);
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var result = await authApiClient.GetSessionAsync(cancellationToken);
        Session = result.Succeeded && result.Value is not null
            ? result.Value
            : new AuthSessionResponse(false, null);
        _initialized = true;
        Changed?.Invoke();
    }

    public void Set(AuthSessionResponse session)
    {
        ArgumentNullException.ThrowIfNull(session);
        Session = session;
        _initialized = true;
        Changed?.Invoke();
    }
}

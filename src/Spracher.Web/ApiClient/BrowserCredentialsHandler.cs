using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Spracher.Web.ApiClient;

public sealed class BrowserCredentialsHandler : DelegatingHandler
{
    public BrowserCredentialsHandler()
        : base(new HttpClientHandler())
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}

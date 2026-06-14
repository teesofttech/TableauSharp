using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Http;
using TableauSharp.Common.Models;
using TableauSharp.Settings;

namespace TableauSharp.Tests.Common;

internal sealed class SiteScopedServiceTestContext : IDisposable
{
    public const string Server = "https://tableau.example.com";
    public const string ApiVersion = "3.23";
    public const string SiteId = "site-luid-123";
    public const string AuthToken = "test-auth-token-abc";

    public MockHttpMessageHandler MockHttp { get; } = new();
    public IHttpClientFactory HttpClientFactory { get; }
    public ITableauRequestBuilder RequestBuilder { get; }
    public string SiteBase => $"{Server}/api/{ApiVersion}/sites/{SiteId}/";

    public SiteScopedServiceTestContext()
    {
        var httpClient = MockHttp.ToHttpClient();

        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient("TableauClient")).Returns(httpClient);
        HttpClientFactory = factory.Object;

        var tokenProvider = new Mock<ITableauTokenProvider>(MockBehavior.Strict);
        tokenProvider.Setup(p => p.GetTokenInfo()).Returns(new AuthToken
        {
            Token = AuthToken,
            SiteId = SiteId,
            SiteContentUrl = "friendly-site",
            UserId = "user-luid-123",
            Expiration = DateTime.UtcNow.AddHours(2)
        });

        RequestBuilder = new TableauRequestBuilder(
            Options.Create(new TableauOptions { Server = Server, Version = ApiVersion }),
            tokenProvider.Object);
    }

    public static bool HasAuthHeader(HttpRequestMessage request)
        => request.Headers.TryGetValues("X-Tableau-Auth", out var values)
            && values.SingleOrDefault() == AuthToken;

    public void Dispose() => MockHttp.Dispose();
}

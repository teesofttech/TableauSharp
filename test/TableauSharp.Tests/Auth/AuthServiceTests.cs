using System.Net;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using TableauSharp.Auth.Service;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Models;
using TableauSharp.Settings;

namespace TableauSharp.Tests.Auth;

[TestFixture]
public class AuthServiceTests
{
    private const string Server = "https://tableau.example.com";
    private const string ApiVersion = "3.23";
    private const string SiteContentUrl = "mysite";
    private const string Token = "auth-token";
    private const string SiteId = "site-luid";
    private const string UserId = "user-luid";

    private MockHttpMessageHandler _mockHttp = null!;
    private Mock<IHttpClientFactory> _mockFactory = null!;
    private TableauTokenProvider _tokenProvider = null!;
    private AuthService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri($"{Server}/api/{ApiVersion}/");

        _mockFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        _mockFactory.Setup(f => f.CreateClient("TableauClient")).Returns(httpClient);

        _tokenProvider = new TableauTokenProvider();

        _service = new AuthService(
            _mockFactory.Object,
            Options.Create(new TableauAuthOptions
            {
                SiteContentUrl = SiteContentUrl,
                PersonalAccessTokenName = "pat-name",
                PersonalAccessTokenSecret = "pat-secret",
                Username = "user",
                Password = "password",
                Jwt_Expiry_Minutes = 10,
                SecretId = "secret-id",
                SecretValue = "this-is-a-test-secret-value-32-bytes"
            }),
            Options.Create(new TableauOptions
            {
                Server = Server,
                Version = ApiVersion,
                Site = SiteContentUrl
            }),
            _tokenProvider);
    }

    [TearDown]
    public void TearDown() => _mockHttp.Dispose();

    [Test]
    public async Task SignInWithPATAsync_StoresTokenAndSiteSession()
    {
        _mockHttp.When(HttpMethod.Post, $"{Server}/api/{ApiVersion}/auth/signin")
            .Respond("application/json", SignInJson);

        var result = await _service.SignInWithPATAsync();
        var stored = _tokenProvider.GetTokenInfo();

        Assert.Multiple(() =>
        {
            Assert.That(result.Token, Is.EqualTo(Token));
            Assert.That(result.SiteId, Is.EqualTo(SiteId));
            Assert.That(result.SiteContentUrl, Is.EqualTo(SiteContentUrl));
            Assert.That(result.UserId, Is.EqualTo(UserId));
            Assert.That(stored.Token, Is.EqualTo(Token));
            Assert.That(stored.SiteId, Is.EqualTo(SiteId));
        });
    }

    [Test]
    public async Task SignInWithUserCredentialsAsync_StoresTokenAndSiteSession()
    {
        _mockHttp.When(HttpMethod.Post, $"{Server}/api/{ApiVersion}/auth/signin")
            .Respond("application/json", SignInJson);

        await _service.SignInWithUserCredentialsAsync();

        Assert.That(_tokenProvider.GetTokenInfo().SiteId, Is.EqualTo(SiteId));
    }

    [Test]
    public async Task SignOutAsync_ClearsTokenAfterSuccessfulSignOut()
    {
        _tokenProvider.SetToken(new AuthToken { Token = Token, SiteId = SiteId });

        _mockHttp.When(HttpMethod.Post, $"{Server}/api/{ApiVersion}/auth/signout")
            .Respond(HttpStatusCode.NoContent);

        await _service.SignOutAsync(Token);

        Assert.Throws<InvalidOperationException>(() => _tokenProvider.GetTokenInfo());
    }

    [Test]
    public async Task SignInWithJWTAsync_SendsRawJwtString()
    {
        string? requestBody = null;

        _mockHttp.When(HttpMethod.Post, $"{Server}/api/{ApiVersion}/auth/signin")
            .With(req =>
            {
                requestBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return true;
            })
            .Respond("application/json", SignInJson);

        await _service.SignInWithJWTAsync("tableau-user");

        Assert.Multiple(() =>
        {
            Assert.That(requestBody, Does.Contain("\"jwt\":\""));
            Assert.That(requestBody, Does.Not.Contain("\"Token\""));
            Assert.That(requestBody, Does.Contain($"\"contentUrl\":\"{SiteContentUrl}\""));
        });
    }

    private static string SignInJson => $$"""
        {
          "credentials": {
            "token": "{{Token}}",
            "estimatedTimeToExpiration": "2:12:30",
            "site": {
              "id": "{{SiteId}}",
              "contentUrl": "{{SiteContentUrl}}"
            },
            "user": {
              "id": "{{UserId}}"
            }
          }
        }
        """;
}

using System.Net;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Http;
using TableauSharp.Common.Models;
using TableauSharp.Settings;
using TableauSharp.Workbooks.Models;
using TableauSharp.Workbooks.Services;

namespace TableauSharp.Tests.Workbooks;

[TestFixture]
public class WorkbookServiceTests
{
    private const string Server = "https://tableau.example.com";
    private const string ApiVersion = "3.23";
    private const string SiteContentUrl = "mysite";
    private const string SiteId = "site-luid-123";
    private const string AuthToken = "test-auth-token-abc";
    private string SiteBase => $"{Server}/api/{ApiVersion}/sites/{SiteId}/";

    private MockHttpMessageHandler _mockHttp = null!;
    private Mock<IHttpClientFactory> _mockFactory = null!;
    private Mock<ITableauTokenProvider> _mockTokenProvider = null!;
    private WorkbookService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();

        var httpClient = _mockHttp.ToHttpClient();

        _mockFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        _mockFactory.Setup(f => f.CreateClient("TableauClient")).Returns(httpClient);

        _mockTokenProvider = new Mock<ITableauTokenProvider>(MockBehavior.Strict);
        _mockTokenProvider.Setup(p => p.GetTokenInfo()).Returns(new AuthToken
        {
            Token = AuthToken,
            SiteId = SiteId,
            SiteContentUrl = SiteContentUrl,
            UserId = "user-luid-123",
            Expiration = DateTime.UtcNow.AddHours(2)
        });

        var tableauOptions = Options.Create(new TableauOptions { Server = Server, Version = ApiVersion });
        var requestBuilder = new TableauRequestBuilder(tableauOptions, _mockTokenProvider.Object);

        _service = new WorkbookService(_mockFactory.Object, requestBuilder);
    }

    [TearDown]
    public void TearDown() => _mockHttp.Dispose();

    // --- GetAllAsync ---

    [Test]
    public async Task GetAllAsync_ReturnsWorkbooks_WhenApiReturnsData()
    {
        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks")
            .Respond("application/json", WorkbookListJson);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("wb-001"));
        Assert.That(result[0].Name, Is.EqualTo("Sales Dashboard"));
        Assert.That(result[0].ProjectId, Is.EqualTo("proj-1"));
        Assert.That(result[0].OwnerId, Is.EqualTo("user-1"));
        Assert.That(result[1].Id, Is.EqualTo("wb-002"));
    }

    [Test]
    public async Task GetAllAsync_SendsAuthTokenHeader()
    {
        string? capturedToken = null;

        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks")
            .With(req =>
            {
                capturedToken = req.Headers.TryGetValues("X-Tableau-Auth", out var vals)
                    ? vals.FirstOrDefault()
                    : null;
                return true;
            })
            .Respond("application/json", WorkbookListJson);

        await _service.GetAllAsync();

        Assert.That(capturedToken, Is.EqualTo(AuthToken));
    }

    [Test]
    public async Task GetAllAsync_CallsTokenProvider_ToGetSession()
    {
        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks")
            .Respond("application/json", WorkbookListJson);

        await _service.GetAllAsync();

        _mockTokenProvider.Verify(p => p.GetTokenInfo(), Times.Once);
    }

    [Test]
    public void GetAllAsync_WhenTokenProviderThrows_PropagatesException()
    {
        _mockTokenProvider.Setup(p => p.GetTokenInfo())
            .Throws(new InvalidOperationException("No Tableau auth token set. Please sign in first."));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllAsync());
        Assert.That(ex!.Message, Does.Contain("No Tableau auth token set"));
    }

    [Test]
    public void GetAllAsync_WhenApiReturns401_ThrowsHttpRequestException()
    {
        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks")
            .Respond(HttpStatusCode.Unauthorized);

        Assert.ThrowsAsync<HttpRequestException>(() => _service.GetAllAsync());
    }

    [Test]
    public void GetAllAsync_WhenCancellationRequested_ThrowsTaskCanceledException()
    {
        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks")
            .Respond("application/json", WorkbookListJson);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(() => _service.GetAllAsync(cts.Token));
    }

    // --- GetByIdAsync ---

    [Test]
    public async Task GetByIdAsync_ReturnsCorrectWorkbook()
    {
        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks/wb-001")
            .Respond("application/json", SingleWorkbookJson("wb-001", "Sales Dashboard"));

        var result = await _service.GetByIdAsync("wb-001");

        Assert.That(result.Id, Is.EqualTo("wb-001"));
        Assert.That(result.Name, Is.EqualTo("Sales Dashboard"));
    }

    [Test]
    public async Task GetByIdAsync_SendsAuthTokenHeader()
    {
        string? capturedToken = null;

        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks/wb-001")
            .With(req =>
            {
                capturedToken = req.Headers.TryGetValues("X-Tableau-Auth", out var vals)
                    ? vals.FirstOrDefault()
                    : null;
                return true;
            })
            .Respond("application/json", SingleWorkbookJson("wb-001", "Sales Dashboard"));

        await _service.GetByIdAsync("wb-001");

        Assert.That(capturedToken, Is.EqualTo(AuthToken));
    }

    [Test]
    public void GetByIdAsync_WhenApiReturns404_ThrowsHttpRequestException()
    {
        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks/missing")
            .Respond(HttpStatusCode.NotFound);

        Assert.ThrowsAsync<HttpRequestException>(() => _service.GetByIdAsync("missing"));
    }

    // --- DeleteAsync ---

    [Test]
    public async Task DeleteAsync_SendsDeleteRequest_ToCorrectUrl()
    {
        var called = false;

        _mockHttp.When(HttpMethod.Delete, $"{SiteBase}workbooks/wb-001")
            .With(_ => { called = true; return true; })
            .Respond(HttpStatusCode.NoContent);

        await _service.DeleteAsync("wb-001");

        Assert.That(called, Is.True);
    }

    [Test]
    public async Task DeleteAsync_SendsAuthTokenHeader()
    {
        string? capturedToken = null;

        _mockHttp.When(HttpMethod.Delete, $"{SiteBase}workbooks/wb-001")
            .With(req =>
            {
                capturedToken = req.Headers.TryGetValues("X-Tableau-Auth", out var vals)
                    ? vals.FirstOrDefault()
                    : null;
                return true;
            })
            .Respond(HttpStatusCode.NoContent);

        await _service.DeleteAsync("wb-001");

        Assert.That(capturedToken, Is.EqualTo(AuthToken));
    }

    // --- Test data helpers ---

    private static string WorkbookListJson => """
        {
          "workbooks": [
            {
              "id": "wb-001",
              "name": "Sales Dashboard",
              "project": { "id": "proj-1" },
              "owner": { "id": "user-1" },
              "createdAt": "2024-01-15T10:00:00Z",
              "updatedAt": "2024-06-01T08:30:00Z"
            },
            {
              "id": "wb-002",
              "name": "Finance Overview",
              "project": { "id": "proj-2" },
              "owner": { "id": "user-2" },
              "createdAt": "2024-02-01T09:00:00Z",
              "updatedAt": "2024-05-20T14:00:00Z"
            }
          ]
        }
        """;

    private static string SingleWorkbookJson(string id, string name) => $$"""
        {
          "workbook": {
            "id": "{{id}}",
            "name": "{{name}}",
            "project": { "id": "proj-1" },
            "owner": { "id": "user-1" },
            "createdAt": "2024-01-15T10:00:00Z",
            "updatedAt": "2024-06-01T08:30:00Z"
          }
        }
        """;
}

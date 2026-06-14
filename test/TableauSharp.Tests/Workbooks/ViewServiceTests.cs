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
public class ViewServiceTests
{
    private const string Server = "https://tableau.example.com";
    private const string ApiVersion = "3.23";
    private const string SiteId = "site-luid-123";
    private const string AuthToken = "test-auth-token-abc";
    private string SiteBase => $"{Server}/api/{ApiVersion}/sites/{SiteId}/";

    private MockHttpMessageHandler _mockHttp = null!;
    private ViewService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();

        var httpClient = _mockHttp.ToHttpClient();
        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient("TableauClient")).Returns(httpClient);

        var tokenProvider = new Mock<ITableauTokenProvider>(MockBehavior.Strict);
        tokenProvider.Setup(p => p.GetTokenInfo()).Returns(new AuthToken
        {
            Token = AuthToken,
            SiteId = SiteId,
            SiteContentUrl = "mysite",
            UserId = "user-luid-123",
            Expiration = DateTime.UtcNow.AddHours(2)
        });

        var requestBuilder = new TableauRequestBuilder(
            Options.Create(new TableauOptions { Server = Server, Version = ApiVersion }),
            tokenProvider.Object);

        _service = new ViewService(factory.Object, requestBuilder);
    }

    [TearDown]
    public void TearDown() => _mockHttp.Dispose();

    [Test]
    public async Task GetViewsByWorkbookIdAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        string? capturedToken = null;

        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks/wb-001/views")
            .With(req =>
            {
                capturedToken = req.Headers.TryGetValues("X-Tableau-Auth", out var values)
                    ? values.SingleOrDefault()
                    : null;
                return true;
            })
            .Respond("application/json", ViewsJson);

        var result = (await _service.GetViewsByWorkbookIdAsync("wb-001")).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("view-001"));
            Assert.That(capturedToken, Is.EqualTo(AuthToken));
        });
    }

    [Test]
    public async Task ExportViewAsync_ReturnsFileContentFromSignedInSite()
    {
        var fileContent = new byte[] { 1, 2, 3 };

        _mockHttp.When(HttpMethod.Get, $"{SiteBase}views/view-001/pdf")
            .Respond(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileContent)
                {
                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf") }
                }
            });

        var result = await _service.ExportViewAsync(new ExportRequest
        {
            ViewId = "view-001",
            Format = "PDF"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.FileContent, Is.EqualTo(fileContent));
            Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(result.FileName, Is.EqualTo("view_view-001.pdf"));
        });
    }

    [Test]
    public void GetViewsByWorkbookIdAsync_WhenCancellationRequested_ThrowsTaskCanceledException()
    {
        _mockHttp.When(HttpMethod.Get, $"{SiteBase}workbooks/wb-001/views")
            .Respond("application/json", ViewsJson);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(() => _service.GetViewsByWorkbookIdAsync("wb-001", cts.Token));
    }

    private static string ViewsJson => """
        {
          "views": [
            {
              "id": "view-001",
              "name": "Sales View",
              "contentUrl": "sales-view",
              "totalViews": 42,
              "lastViewedAt": "2024-06-01T08:30:00Z"
            }
          ]
        }
        """;
}

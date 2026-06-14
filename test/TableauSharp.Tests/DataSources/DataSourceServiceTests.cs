using RichardSzalay.MockHttp;
using TableauSharp.DataSources.Services;
using TableauSharp.Tests.Common;

namespace TableauSharp.Tests.DataSources;

[TestFixture]
public class DataSourceServiceTests
{
    private SiteScopedServiceTestContext _context = null!;
    private DataSourceService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new SiteScopedServiceTestContext();
        _service = new DataSourceService(_context.HttpClientFactory, _context.RequestBuilder);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetAllAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        _context.MockHttp.When(HttpMethod.Get, $"{_context.SiteBase}datasources")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond("application/json", DataSourcesJson);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("ds-001"));
        });
    }

    [Test]
    public async Task RefreshAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        var called = false;

        _context.MockHttp.When(HttpMethod.Post, $"{_context.SiteBase}datasources/ds-001/refresh")
            .With(req =>
            {
                called = SiteScopedServiceTestContext.HasAuthHeader(req);
                return called;
            })
            .Respond(System.Net.HttpStatusCode.Accepted);

        await _service.RefreshAsync("ds-001");

        Assert.That(called, Is.True);
    }

    private static string DataSourcesJson => """
        {
          "datasources": [
            {
              "id": "ds-001",
              "name": "Sales Data",
              "project": { "id": "proj-001" },
              "owner": { "id": "user-001" },
              "type": "hyper",
              "createdAt": "2024-01-15T10:00:00Z",
              "updatedAt": "2024-06-01T08:30:00Z",
              "isCertified": true,
              "contentUrl": "sales-data"
            }
          ]
        }
        """;
}

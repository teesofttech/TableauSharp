using RichardSzalay.MockHttp;
using TableauSharp.DataSources.Services;
using TableauSharp.Tests.Common;

namespace TableauSharp.Tests.DataSources;

[TestFixture]
public class DataSourceServiceTests
{
    private SiteScopedServiceTestContext _context = null!;
    private DataSourceService _service = null!;
    private readonly List<string> _tempFiles = [];

    [SetUp]
    public void SetUp()
    {
        _context = new SiteScopedServiceTestContext();
        _service = new DataSourceService(_context.HttpClientFactory, _context.RequestBuilder);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        foreach (var file in _tempFiles)
        {
            File.Delete(file);
        }
        _tempFiles.Clear();
    }

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

    [Test]
    public async Task PublishAsync_SendsTableauMultipartMixedRequest()
    {
        var filePath = CreateTempFile("datasource.tdsx", "datasource-content");
        string? contentType = null;
        string? requestBody = null;

        _context.MockHttp.When(HttpMethod.Post, $"{_context.SiteBase}datasources?overwrite=true")
            .With(req =>
            {
                contentType = req.Content!.Headers.ContentType?.MediaType;
                requestBody = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return SiteScopedServiceTestContext.HasAuthHeader(req);
            })
            .Respond("application/json", SingleDataSourceJson);

        var result = await _service.PublishAsync(new()
        {
            Name = "Sales & Data",
            ProjectId = "proj-001",
            Description = "A <trusted> source",
            FilePath = filePath,
            Overwrite = true
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo("ds-001"));
            Assert.That(contentType, Is.EqualTo("multipart/mixed"));
            Assert.That(requestBody, Does.Contain("request_payload"));
            Assert.That(requestBody, Does.Contain("tableau_datasource"));
            Assert.That(requestBody, Does.Contain("filename=\"datasource.tdsx\""));
            Assert.That(requestBody, Does.Contain("<datasource name=\"Sales &amp; Data\" description=\"A &lt;trusted&gt; source\">"));
            Assert.That(requestBody, Does.Contain("<project id=\"proj-001\""));
            Assert.That(requestBody, Does.Not.Contain("datasourceName"));
        });
    }

    [Test]
    public void PublishAsync_WhenFileMissing_ThrowsFileNotFoundException()
    {
        var missingPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid()}.tdsx");

        Assert.ThrowsAsync<FileNotFoundException>(() => _service.PublishAsync(new()
        {
            Name = "Sales Data",
            ProjectId = "proj-001",
            FilePath = missingPath
        }));
    }

    [Test]
    public void PublishAsync_WhenDataSourceExtensionUnsupported_ThrowsNotSupportedException()
    {
        var filePath = CreateTempFile("datasource.csv", "not-a-supported-datasource");

        var ex = Assert.ThrowsAsync<NotSupportedException>(() => _service.PublishAsync(new()
        {
            Name = "Sales Data",
            ProjectId = "proj-001",
            FilePath = filePath
        }));

        Assert.That(ex!.Message, Does.Contain("extension"));
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

    private static string SingleDataSourceJson => """
        {
          "datasource": {
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
        }
        """;

    private string CreateTempFile(string fileName, string content)
    {
        var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }
}

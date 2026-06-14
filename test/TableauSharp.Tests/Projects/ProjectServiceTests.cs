using RichardSzalay.MockHttp;
using TableauSharp.Projects.Models;
using TableauSharp.Projects.Services;
using TableauSharp.Tests.Common;

namespace TableauSharp.Tests.Projects;

[TestFixture]
public class ProjectServiceTests
{
    private SiteScopedServiceTestContext _context = null!;
    private ProjectService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new SiteScopedServiceTestContext();
        _service = new ProjectService(_context.HttpClientFactory, _context.RequestBuilder);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetAllAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        _context.MockHttp.When(HttpMethod.Get, $"{_context.SiteBase}projects")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond("application/json", ProjectsJson);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("proj-001"));
        });
    }

    [Test]
    public async Task GetAllAsync_WhenOptionalFieldsAreMissing_ReturnsNullOptionalValues()
    {
        _context.MockHttp.When(HttpMethod.Get, $"{_context.SiteBase}projects")
            .Respond("application/json", ProjectWithoutOptionalFieldsJson);

        var result = (await _service.GetAllAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo("proj-002"));
            Assert.That(result.Description, Is.Null);
            Assert.That(result.ParentProjectId, Is.Null);
        });
    }

    [Test]
    public async Task UpdateAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        _context.MockHttp.When(HttpMethod.Put, $"{_context.SiteBase}projects/proj-001")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond("application/json", SingleProjectJson);

        var result = await _service.UpdateAsync("proj-001", new ProjectUpdateRequest
        {
            Name = "Renamed",
            Description = "Updated"
        });

        Assert.That(result.Id, Is.EqualTo("proj-001"));
    }

    private static string ProjectsJson => """
        {
          "projects": [
            {
              "id": "proj-001",
              "name": "Default",
              "description": "Project",
              "owner": { "id": "user-001" }
            }
          ]
        }
        """;

    private static string SingleProjectJson => """
        {
          "project": {
            "id": "proj-001",
            "name": "Renamed",
            "description": "Updated",
            "owner": { "id": "user-001" }
          }
        }
        """;

    private static string ProjectWithoutOptionalFieldsJson => """
        {
          "projects": [
            {
              "id": "proj-002",
              "name": "No optional fields",
              "owner": { "id": "user-001" }
            }
          ]
        }
        """;
}

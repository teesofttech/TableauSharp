using RichardSzalay.MockHttp;
using TableauSharp.Permissions.Models;
using TableauSharp.Permissions.Services;
using TableauSharp.Tests.Common;

namespace TableauSharp.Tests.Permissions;

[TestFixture]
public class PermissionServiceTests
{
    private SiteScopedServiceTestContext _context = null!;
    private PermissionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new SiteScopedServiceTestContext();
        _service = new PermissionService(_context.HttpClientFactory, _context.RequestBuilder);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetWorkbookPermissionsAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        _context.MockHttp.When(HttpMethod.Get, $"{_context.SiteBase}workbooks/wb-001/permissions")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond("application/json", PermissionsJson);

        var result = (await _service.GetWorkbookPermissionsAsync("wb-001")).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].GranteeId, Is.EqualTo("user-001"));
        });
    }

    [Test]
    public async Task DeleteProjectPermissionAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        var called = false;

        _context.MockHttp.When(HttpMethod.Delete, $"{_context.SiteBase}projects/proj-001/permissions/users/user-001/Read")
            .With(req =>
            {
                called = SiteScopedServiceTestContext.HasAuthHeader(req);
                return called;
            })
            .Respond(System.Net.HttpStatusCode.NoContent);

        await _service.DeleteProjectPermissionAsync("proj-001", "user-001", "User", "Read");

        Assert.That(called, Is.True);
    }

    [Test]
    public async Task AddDataSourcePermissionAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        var permission = new TableauPermission
        {
            GranteeId = "group-001",
            GranteeType = "Group"
        };

        _context.MockHttp.When(HttpMethod.Put, $"{_context.SiteBase}datasources/ds-001/permissions")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond(System.Net.HttpStatusCode.OK);

        await _service.AddDataSourcePermissionAsync("ds-001", permission);

        Assert.Pass();
    }

    private static string PermissionsJson => """
        {
          "permissions": {
            "granteeCapabilities": [
              {
                "user": { "id": "user-001" },
                "capabilities": {
                  "capability": [
                    { "name": "Read", "mode": "Allow" }
                  ]
                }
              }
            ]
          }
        }
        """;
}

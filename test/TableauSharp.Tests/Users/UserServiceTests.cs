using RichardSzalay.MockHttp;
using TableauSharp.Tests.Common;
using TableauSharp.Users.Models;
using TableauSharp.Users.Services;

namespace TableauSharp.Tests.Users;

[TestFixture]
public class UserServiceTests
{
    private SiteScopedServiceTestContext _context = null!;
    private UserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new SiteScopedServiceTestContext();
        _service = new UserService(_context.HttpClientFactory, _context.RequestBuilder);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetAllAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        _context.MockHttp.When(HttpMethod.Get, $"{_context.SiteBase}users")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond("application/json", UsersJson);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("user-001"));
        });
    }

    [Test]
    public async Task GetAllAsync_WhenEmailIsMissing_ReturnsNullEmail()
    {
        _context.MockHttp.When(HttpMethod.Get, $"{_context.SiteBase}users")
            .Respond("application/json", UserWithoutEmailJson);

        var result = (await _service.GetAllAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo("user-002"));
            Assert.That(result.Email, Is.Null);
        });
    }

    [Test]
    public async Task UpdateAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        _context.MockHttp.When(HttpMethod.Put, $"{_context.SiteBase}users/user-001")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond("application/json", SingleUserJson);

        var result = await _service.UpdateAsync("user-001", new UserUpdateRequest { SiteRole = "Viewer" });

        Assert.That(result.Id, Is.EqualTo("user-001"));
    }

    private static string UsersJson => """
        {
          "users": [
            {
              "id": "user-001",
              "name": "alice",
              "email": "alice@example.com",
              "siteRole": "Viewer"
            }
          ]
        }
        """;

    private static string SingleUserJson => """
        {
          "user": {
            "id": "user-001",
            "name": "alice",
            "email": "alice@example.com",
            "siteRole": "Viewer"
          }
        }
        """;

    private static string UserWithoutEmailJson => """
        {
          "users": [
            {
              "id": "user-002",
              "name": "bob",
              "siteRole": "Viewer"
            }
          ]
        }
        """;
}

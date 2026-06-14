using RichardSzalay.MockHttp;
using TableauSharp.Tests.Common;
using TableauSharp.Users.Services;

namespace TableauSharp.Tests.Users;

[TestFixture]
public class GroupServiceTests
{
    private SiteScopedServiceTestContext _context = null!;
    private GroupService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new SiteScopedServiceTestContext();
        _service = new GroupService(_context.HttpClientFactory, _context.RequestBuilder);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetAllAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        _context.MockHttp.When(HttpMethod.Get, $"{_context.SiteBase}groups")
            .With(SiteScopedServiceTestContext.HasAuthHeader)
            .Respond("application/json", GroupsJson);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("group-001"));
        });
    }

    [Test]
    public async Task AddUserToGroupAsync_UsesSignedInSiteIdAndAuthHeader()
    {
        var called = false;

        _context.MockHttp.When(HttpMethod.Post, $"{_context.SiteBase}groups/group-001/users")
            .With(req =>
            {
                called = SiteScopedServiceTestContext.HasAuthHeader(req);
                return called;
            })
            .Respond(System.Net.HttpStatusCode.NoContent);

        await _service.AddUserToGroupAsync("group-001", "user-001");

        Assert.That(called, Is.True);
    }

    private static string GroupsJson => """
        {
          "groups": [
            {
              "id": "group-001",
              "name": "Analysts"
            }
          ]
        }
        """;
}

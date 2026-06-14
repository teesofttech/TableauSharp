using TableauSharp.Common.Helper;
using TableauSharp.Common.Models;

namespace TableauSharp.Tests.Common;

[TestFixture]
public class TableauTokenProviderTests
{
    [Test]
    public void GetTokenInfo_WhenTokenNotSet_ThrowsClearException()
    {
        var provider = new TableauTokenProvider();

        var ex = Assert.Throws<InvalidOperationException>(() => provider.GetTokenInfo());

        Assert.That(ex!.Message, Does.Contain("No Tableau auth token set"));
    }

    [Test]
    public void SetToken_WithAuthToken_StoresFullSession()
    {
        var provider = new TableauTokenProvider();

        provider.SetToken(new AuthToken
        {
            Token = "auth-token",
            SiteId = "site-luid",
            SiteContentUrl = "mysite",
            UserId = "user-luid",
            Expiration = DateTime.UtcNow.AddHours(1)
        });

        var session = provider.GetTokenInfo();

        Assert.Multiple(() =>
        {
            Assert.That(session.Token, Is.EqualTo("auth-token"));
            Assert.That(session.SiteId, Is.EqualTo("site-luid"));
            Assert.That(session.SiteContentUrl, Is.EqualTo("mysite"));
            Assert.That(session.UserId, Is.EqualTo("user-luid"));
        });
    }
}

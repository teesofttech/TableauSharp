using Microsoft.Extensions.Options;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Http;
using TableauSharp.Common.Models;
using TableauSharp.Settings;

namespace TableauSharp.Tests.Common;

[TestFixture]
public class TableauRequestBuilderTests
{
    [Test]
    public void CreateSiteRequest_UsesSignedInSiteIdAndAuthHeader()
    {
        var tokenProvider = new TableauTokenProvider();
        tokenProvider.SetToken(new AuthToken
        {
            Token = "auth-token",
            SiteId = "site-luid-123",
            SiteContentUrl = "friendly-site"
        });

        var builder = new TableauRequestBuilder(
            Options.Create(new TableauOptions
            {
                Server = "https://tableau.example.com/",
                Version = "3.23"
            }),
            tokenProvider);

        using var request = builder.CreateSiteRequest(HttpMethod.Get, "/workbooks");

        Assert.Multiple(() =>
        {
            Assert.That(request.RequestUri!.ToString(), Is.EqualTo("https://tableau.example.com/api/3.23/sites/site-luid-123/workbooks"));
            Assert.That(request.Headers.GetValues("X-Tableau-Auth").Single(), Is.EqualTo("auth-token"));
        });
    }

    [Test]
    public void CreateSiteRequest_WhenSiteIdMissing_ThrowsClearException()
    {
        var tokenProvider = new TableauTokenProvider();
        tokenProvider.SetToken("auth-token");

        var builder = new TableauRequestBuilder(
            Options.Create(new TableauOptions { Server = "https://tableau.example.com", Version = "3.23" }),
            tokenProvider);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            builder.CreateSiteRequest(HttpMethod.Get, "workbooks"));

        Assert.That(ex!.Message, Does.Contain("site id"));
    }
}

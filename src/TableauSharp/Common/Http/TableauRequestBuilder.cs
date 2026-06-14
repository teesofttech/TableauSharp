using Microsoft.Extensions.Options;
using TableauSharp.Common.Helper;
using TableauSharp.Settings;

namespace TableauSharp.Common.Http;

public class TableauRequestBuilder(
    IOptions<TableauOptions> options,
    ITableauTokenProvider tokenProvider) : ITableauRequestBuilder
{
    private readonly TableauOptions _options = options.Value;
    private readonly ITableauTokenProvider _tokenProvider = tokenProvider;

    public HttpRequestMessage CreateSiteRequest(HttpMethod method, string relativePath)
    {
        var session = _tokenProvider.GetTokenInfo();
        if (string.IsNullOrWhiteSpace(session.SiteId))
        {
            throw new InvalidOperationException("No Tableau site id set. Please sign in first.");
        }

        var request = new HttpRequestMessage(
            method,
            BuildUri($"api/{_options.Version}/sites/{session.SiteId}/{relativePath.TrimStart('/')}"));
        request.Headers.Add("X-Tableau-Auth", session.Token);
        return request;
    }

    public HttpRequestMessage CreateServerRequest(HttpMethod method, string relativePath)
    {
        var session = _tokenProvider.GetTokenInfo();
        var request = new HttpRequestMessage(method, BuildUri(relativePath));
        request.Headers.Add("X-Tableau-Auth", session.Token);
        return request;
    }

    private Uri BuildUri(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(_options.Server))
        {
            throw new InvalidOperationException("TableauOptions.Server must be configured.");
        }

        var server = _options.Server.TrimEnd('/');
        var path = relativePath.TrimStart('/');
        return new Uri($"{server}/{path}");
    }
}

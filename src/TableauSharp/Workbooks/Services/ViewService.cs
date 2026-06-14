using System.Text.Json;
using TableauSharp.Common.Http;
using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public class ViewService : IViewService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauRequestBuilder _requestBuilder;

    public ViewService(
        IHttpClientFactory httpClientFactory,
        ITableauRequestBuilder requestBuilder)
    {
        _httpClientFactory = httpClientFactory;
        _requestBuilder = requestBuilder;
    }

    public async Task<IEnumerable<TableauView>> GetViewsByWorkbookIdAsync(string workbookId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");

        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, $"workbooks/{workbookId}/views");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var views = new List<TableauView>();
        foreach (var v in doc.RootElement.GetProperty("views").EnumerateArray())
        {
            views.Add(new TableauView
            {
                Id = v.GetProperty("id").GetString(),
                Name = v.GetProperty("name").GetString(),
                ContentUrl = v.GetProperty("contentUrl").GetString(),
                WorkbookId = workbookId,
                TotalViews = v.GetProperty("totalViews").GetInt32(),
                LastViewedAt = v.TryGetProperty("lastViewedAt", out var lastViewed)
                    ? lastViewed.GetDateTime()
                    : default
            });

        }

        return views;
    }

    public async Task<ExportResponse> ExportViewAsync(ExportRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");

        using var httpRequest = _requestBuilder.CreateSiteRequest(HttpMethod.Get, $"views/{request.ViewId}/{request.Format.ToLower()}");
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return new ExportResponse
        {
            FileContent = bytes,
            ContentType = contentType,
            FileName = $"view_{request.ViewId}.{request.Format.ToLower()}"
        };
    }
}

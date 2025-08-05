using System.Text.Json;
using Microsoft.Extensions.Options;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Models;
using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public class ViewService : IViewService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TableauAuthOptions _options;
    private readonly ITableauTokenProvider _tokenProvider;

    public ViewService(
        IHttpClientFactory httpClientFactory,
        IOptions<TableauAuthOptions> options,
        ITableauTokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _tokenProvider = tokenProvider;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        client.BaseAddress = new Uri($"{_options.ServerUrl}/api/3.20/sites/{_options.SiteContentUrl}/");
        client.DefaultRequestHeaders.Add("X-Tableau-Auth", _tokenProvider.GetToken());
        return client;
    }

    public async Task<IEnumerable<TableauView>> GetViewsByWorkbookIdAsync(string workbookId)
    {
        using var client = CreateClient();

        var response = await client.GetAsync($"workbooks/{workbookId}/views");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
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

    public async Task<ExportResponse> ExportViewAsync(ExportRequest request)
    {
        using var client = CreateClient();

        var response = await client.GetAsync($"views/{request.ViewId}/{request.Format.ToLower()}");
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return new ExportResponse
        {
            FileContent = bytes,
            ContentType = contentType,
            FileName = $"view_{request.ViewId}.{request.Format.ToLower()}"
        };
    }
}

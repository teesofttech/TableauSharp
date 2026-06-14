using System.Text.Json;
using TableauSharp.Common.Http;
using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public class WorkbookService : IWorkbookService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauRequestBuilder _requestBuilder;

    public WorkbookService(
        IHttpClientFactory httpClientFactory,
        ITableauRequestBuilder requestBuilder)
    {
        _httpClientFactory = httpClientFactory;
        _requestBuilder = requestBuilder;
    }

    public async Task<IEnumerable<TableauWorkbook>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, "workbooks");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var workbooks = new List<TableauWorkbook>();
        foreach (var w in doc.RootElement.GetProperty("workbooks").EnumerateArray())
        {
            workbooks.Add(new TableauWorkbook
            {
                Id = w.GetProperty("id").GetString() ?? string.Empty,
                Name = w.GetProperty("name").GetString() ?? string.Empty,
                ProjectId = w.GetProperty("project").GetProperty("id").GetString() ?? string.Empty,
                OwnerId = w.GetProperty("owner").GetProperty("id").GetString(),
                CreatedAt = w.GetProperty("createdAt").GetDateTime(),
                UpdatedAt = w.GetProperty("updatedAt").GetDateTime()
            });
        }

        return workbooks;
    }

    public async Task<TableauWorkbook> GetByIdAsync(string workbookId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");

        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, $"workbooks/{workbookId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var w = doc.RootElement.GetProperty("workbook");

        return new TableauWorkbook
        {
            Id = w.GetProperty("id").GetString() ?? string.Empty,
            Name = w.GetProperty("name").GetString() ?? string.Empty,
            ProjectId = w.GetProperty("project").GetProperty("id").GetString() ?? string.Empty,
            OwnerId = w.GetProperty("owner").GetProperty("id").GetString(),
            CreatedAt = w.GetProperty("createdAt").GetDateTime(),
            UpdatedAt = w.GetProperty("updatedAt").GetDateTime()
        };
    }


    public async Task<TableauWorkbook> PublishAsync(WorkbookPublishRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");

        ArgumentNullException.ThrowIfNull(request);

        using var content = await TableauPublishContentBuilder.CreateWorkbookContentAsync(
            request.Name,
            request.ProjectId,
            request.FilePath,
            cancellationToken);
        using var httpRequest = _requestBuilder.CreateSiteRequest(
            HttpMethod.Post,
            $"workbooks?overwrite={request.Overwrite.ToString().ToLowerInvariant()}");
        httpRequest.Content = content;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var w = doc.RootElement.GetProperty("workbook");

        return new TableauWorkbook
        {
            Id = w.GetProperty("id").GetString() ?? string.Empty,
            Name = w.GetProperty("name").GetString() ?? string.Empty,
            ProjectId = w.GetProperty("project").GetProperty("id").GetString() ?? string.Empty,
            OwnerId = w.GetProperty("owner").GetProperty("id").GetString(),
            CreatedAt = w.GetProperty("createdAt").GetDateTime(),
            UpdatedAt = w.GetProperty("updatedAt").GetDateTime()
        };
    }

    public async Task DeleteAsync(string workbookId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");

        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Delete, $"workbooks/{workbookId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }


}

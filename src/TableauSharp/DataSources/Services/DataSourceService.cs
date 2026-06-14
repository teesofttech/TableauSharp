using System.Text.Json;
using TableauSharp.Common.Http;
using TableauSharp.DataSources.Models;

namespace TableauSharp.DataSources.Services;

public class DataSourceService : IDataSourceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauRequestBuilder _requestBuilder;

    public DataSourceService(
        IHttpClientFactory httpClientFactory,
        ITableauRequestBuilder requestBuilder)
    {
        _httpClientFactory = httpClientFactory;
        _requestBuilder = requestBuilder;
    }

    public async Task<IEnumerable<TableauDataSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, "datasources");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var dataSources = new List<TableauDataSource>();
        if (doc.RootElement.TryGetProperty("datasources", out var datasourcesElement))
        {
            foreach (var ds in datasourcesElement.EnumerateArray())
            {
                dataSources.Add(new TableauDataSource
                {
                    Id = ds.GetProperty("id").GetString() ?? string.Empty,
                    Name = ds.GetProperty("name").GetString() ?? string.Empty,
                    ProjectId = ds.GetProperty("project").GetProperty("id").GetString() ?? string.Empty,
                    OwnerId = ds.GetProperty("owner").GetProperty("id").GetString() ?? string.Empty,
                    Type = ds.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null,
                    CreatedAt = ds.GetProperty("createdAt").GetDateTime(),
                    UpdatedAt = ds.GetProperty("updatedAt").GetDateTime(),
                    IsCertified = ds.TryGetProperty("isCertified", out var certEl) && certEl.GetBoolean(),
                    ContentUrl = ds.TryGetProperty("contentUrl", out var urlEl) ? urlEl.GetString() : null
                });
            }
        }

        return dataSources;
    }

    public async Task<TableauDataSource> GetByIdAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, $"datasources/{dataSourceId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var ds = doc.RootElement.GetProperty("datasource");

        return new TableauDataSource
        {
            Id = ds.GetProperty("id").GetString() ?? string.Empty,
            Name = ds.GetProperty("name").GetString() ?? string.Empty,
            ProjectId = ds.GetProperty("project").GetProperty("id").GetString() ?? string.Empty,
            OwnerId = ds.GetProperty("owner").GetProperty("id").GetString() ?? string.Empty,
            Type = ds.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null,
            CreatedAt = ds.GetProperty("createdAt").GetDateTime(),
            UpdatedAt = ds.GetProperty("updatedAt").GetDateTime(),
            IsCertified = ds.TryGetProperty("isCertified", out var certEl) && certEl.GetBoolean(),
            ContentUrl = ds.TryGetProperty("contentUrl", out var urlEl) ? urlEl.GetString() : null
        };
    }

    public async Task<TableauDataSource> PublishAsync(DataSourcePublishRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(request.ProjectId), "projectId");
        form.Add(new StringContent(request.Name), "datasourceName");
        form.Add(new StringContent(request.Overwrite.ToString().ToLower()), "overwrite");

        if (!string.IsNullOrEmpty(request.Description))
        {
            form.Add(new StringContent(request.Description), "description");
        }

        // Attach data source file
        var fileBytes = await File.ReadAllBytesAsync(request.FilePath, cancellationToken);
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "tableau_datasource", Path.GetFileName(request.FilePath));

        using var httpRequest = _requestBuilder.CreateSiteRequest(HttpMethod.Post, "datasources");
        httpRequest.Content = form;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var ds = doc.RootElement.GetProperty("datasource");

        return new TableauDataSource
        {
            Id = ds.GetProperty("id").GetString() ?? string.Empty,
            Name = ds.GetProperty("name").GetString() ?? string.Empty,
            ProjectId = ds.GetProperty("project").GetProperty("id").GetString() ?? string.Empty,
            OwnerId = ds.GetProperty("owner").GetProperty("id").GetString() ?? string.Empty,
            Type = ds.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null,
            CreatedAt = ds.GetProperty("createdAt").GetDateTime(),
            UpdatedAt = ds.GetProperty("updatedAt").GetDateTime(),
            IsCertified = ds.TryGetProperty("isCertified", out var certEl) && certEl.GetBoolean(),
            ContentUrl = ds.TryGetProperty("contentUrl", out var urlEl) ? urlEl.GetString() : null
        };
    }

    public async Task DeleteAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Delete, $"datasources/{dataSourceId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RefreshAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Post, $"datasources/{dataSourceId}/refresh");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

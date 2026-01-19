using Microsoft.Extensions.Options;
using System.Text.Json;
using TableauSharp.Common.Helper;
using TableauSharp.DataSources.Models;
using TableauSharp.Settings;

namespace TableauSharp.DataSources.Services;

public class DataSourceService : IDataSourceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauTokenProvider _tokenProvider;
    private readonly TableauAuthOptions _authOptions;
    private readonly TableauOptions _tableauOptions;

    public DataSourceService(
        IHttpClientFactory httpClientFactory,
        ITableauTokenProvider tokenProvider,
        IOptions<TableauAuthOptions> authOptions,
        IOptions<TableauOptions> tableauOptions)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
        _authOptions = authOptions.Value;
        _tableauOptions = tableauOptions.Value;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        client.BaseAddress = new Uri($"{_tableauOptions.Server}/api/{_tableauOptions.Version}/sites/{_authOptions.SiteContentUrl}/");
        client.DefaultRequestHeaders.Add("X-Tableau-Auth", _tokenProvider.GetToken());
        return client;
    }

    public async Task<IEnumerable<TableauDataSource>> GetAllAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("datasources");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
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

    public async Task<TableauDataSource> GetByIdAsync(string dataSourceId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"datasources/{dataSourceId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
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

    public async Task<TableauDataSource> PublishAsync(DataSourcePublishRequest request)
    {
        using var client = CreateClient();
        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(request.ProjectId), "projectId");
        form.Add(new StringContent(request.Name), "datasourceName");
        form.Add(new StringContent(request.Overwrite.ToString().ToLower()), "overwrite");

        if (!string.IsNullOrEmpty(request.Description))
        {
            form.Add(new StringContent(request.Description), "description");
        }

        // Attach data source file
        var fileBytes = await File.ReadAllBytesAsync(request.FilePath);
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "tableau_datasource", Path.GetFileName(request.FilePath));

        var response = await client.PostAsync("datasources", form);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
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

    public async Task DeleteAsync(string dataSourceId)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"datasources/{dataSourceId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task RefreshAsync(string dataSourceId)
    {
        using var client = CreateClient();
        var response = await client.PostAsync($"datasources/{dataSourceId}/refresh", null);
        response.EnsureSuccessStatusCode();
    }
}

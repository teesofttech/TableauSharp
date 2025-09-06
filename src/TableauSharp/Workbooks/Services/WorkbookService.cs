using Microsoft.Extensions.Options;
using System.Text.Json;
using TableauSharp.Common.Models;
using TableauSharp.Settings;
using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public class WorkbookService : IWorkbookService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TableauAuthOptions _options;
    private readonly string _token;
    private readonly TableauOptions _tableauOptions;

    public WorkbookService(IHttpClientFactory httpClientFactory, IOptions<TableauAuthOptions> options,
        IOptions<TableauOptions> tableauOptions)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _tableauOptions = tableauOptions.Value;
    }

    // Helper method to create client per request with proper headers
    private HttpClient CreateClient(string token)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        client.BaseAddress = new Uri($"{_tableauOptions.Url}/api/{_tableauOptions.Version}/sites/{_options.SiteContentUrl}/");
        client.DefaultRequestHeaders.Add("X-Tableau-Auth", token);
        return client;
    }

    public async Task<IEnumerable<TableauWorkbook>> GetAllAsync()
    {
        using var client = CreateClient(_token);
        var response = await client.GetAsync("workbooks");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var workbooks = new List<TableauWorkbook>();
        foreach (var w in doc.RootElement.GetProperty("workbooks").EnumerateArray())
        {
            workbooks.Add(new TableauWorkbook
            {
                Id = w.GetProperty("id").GetString(),
                Name = w.GetProperty("name").GetString(),
                ProjectId = w.GetProperty("project").GetProperty("id").GetString(),
                OwnerId = w.GetProperty("owner").GetProperty("id").GetString(),
                CreatedAt = w.GetProperty("createdAt").GetDateTime(),
                UpdatedAt = w.GetProperty("updatedAt").GetDateTime()
            });
        }

        return workbooks;
    }

    public async Task<TableauWorkbook> GetByIdAsync(string workbookId)
    {
        using var client = CreateClient(_token); // CreateClient() from earlier example

        var response = await client.GetAsync($"workbooks/{workbookId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var w = doc.RootElement.GetProperty("workbook");

        return new TableauWorkbook
        {
            Id = w.GetProperty("id").GetString(),
            Name = w.GetProperty("name").GetString(),
            ProjectId = w.GetProperty("project").GetProperty("id").GetString(),
            OwnerId = w.GetProperty("owner").GetProperty("id").GetString(),
            CreatedAt = w.GetProperty("createdAt").GetDateTime(),
            UpdatedAt = w.GetProperty("updatedAt").GetDateTime()
        };
    }


    public async Task<TableauWorkbook> PublishAsync(WorkbookPublishRequest request)
    {
        using var client = CreateClient(_token);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(request.ProjectId), "projectId");
        form.Add(new StringContent(request.Name), "workbookName");
        form.Add(new StringContent(request.Overwrite.ToString().ToLower()), "overwrite");

        // Attach workbook file
        var fileBytes = await System.IO.File.ReadAllBytesAsync(request.FilePath);
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "tableau_workbook", Path.GetFileName(request.FilePath));

        var response = await client.PostAsync("workbooks", form);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var w = doc.RootElement.GetProperty("workbook");

        return new TableauWorkbook
        {
            Id = w.GetProperty("id").GetString(),
            Name = w.GetProperty("name").GetString(),
            ProjectId = w.GetProperty("project").GetProperty("id").GetString(),
            OwnerId = w.GetProperty("owner").GetProperty("id").GetString(),
            CreatedAt = w.GetProperty("createdAt").GetDateTime(),
            UpdatedAt = w.GetProperty("updatedAt").GetDateTime()
        };
    }

    public async Task DeleteAsync(string workbookId)
    {
        using var client = CreateClient(_token);

        var response = await client.DeleteAsync($"workbooks/{workbookId}");
        response.EnsureSuccessStatusCode();
    }


}
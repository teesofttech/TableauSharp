using System.Net.Http.Headers;
using System.Security;
using System.Text;

namespace TableauSharp.Common.Http;

public static class TableauPublishContentBuilder
{
    private static readonly HashSet<string> WorkbookExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".twb",
        ".twbx"
    };

    private static readonly HashSet<string> DataSourceExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".hyper",
        ".tds",
        ".tdsx",
        ".tde"
    };

    public static async Task<HttpContent> CreateWorkbookContentAsync(
        string name,
        string projectId,
        string filePath,
        CancellationToken cancellationToken)
    {
        ValidatePublishRequest(name, projectId, filePath, WorkbookExtensions, "Workbook");

        var payload = $"""
            <tsRequest>
              <workbook name="{Escape(name)}">
                <project id="{Escape(projectId)}" />
              </workbook>
            </tsRequest>
            """;

        return await CreateMultipartContentAsync(
            payload,
            "tableau_workbook",
            filePath,
            cancellationToken);
    }

    public static async Task<HttpContent> CreateDataSourceContentAsync(
        string name,
        string projectId,
        string? description,
        string filePath,
        CancellationToken cancellationToken)
    {
        ValidatePublishRequest(name, projectId, filePath, DataSourceExtensions, "Data source");

        var descriptionAttribute = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : $" description=\"{Escape(description)}\"";
        var payload = $"""
            <tsRequest>
              <datasource name="{Escape(name)}"{descriptionAttribute}>
                <project id="{Escape(projectId)}" />
              </datasource>
            </tsRequest>
            """;

        return await CreateMultipartContentAsync(
            payload,
            "tableau_datasource",
            filePath,
            cancellationToken);
    }

    private static async Task<HttpContent> CreateMultipartContentAsync(
        string payload,
        string filePartName,
        string filePath,
        CancellationToken cancellationToken)
    {
        var content = new MultipartContent("mixed");

        var payloadContent = new StringContent(payload, Encoding.UTF8, "text/xml");
        payloadContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "\"request_payload\""
        };
        content.Add(payloadContent);

        var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath, cancellationToken));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = $"\"{filePartName}\"",
            FileName = $"\"{Path.GetFileName(filePath)}\""
        };
        content.Add(fileContent);

        return content;
    }

    private static void ValidatePublishRequest(
        string name,
        string projectId,
        string filePath,
        HashSet<string> allowedExtensions,
        string resourceName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"{resourceName} name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new ArgumentException("Project id is required.", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Publish file was not found.", filePath);
        }

        var extension = Path.GetExtension(filePath);
        if (!allowedExtensions.Contains(extension))
        {
            throw new NotSupportedException($"{resourceName} file extension '{extension}' is not supported.");
        }
    }

    private static string Escape(string value)
        => SecurityElement.Escape(value) ?? string.Empty;
}

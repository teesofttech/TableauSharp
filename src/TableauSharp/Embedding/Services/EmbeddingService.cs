using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TableauSharp.Common.Helper;
using TableauSharp.Embedding.Models;
using TableauSharp.Settings;

namespace TableauSharp.Embedding.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauTokenProvider _tokenProvider;
    private readonly TableauAuthOptions _authOptions;
    private readonly TableauOptions _tableauOptions;

    public EmbeddingService(
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
        client.BaseAddress = new Uri($"{_tableauOptions.Server}/");
        client.DefaultRequestHeaders.Add("X-Tableau-Auth", _tokenProvider.GetToken());
        return client;
    }

    public async Task<TrustedTicketResponse> GetTrustedTicketAsync(TrustedTicketRequest request)
    {
        using var client = CreateClient();

        var form = new Dictionary<string, string>
        {
            { "username", request.Username },
            { "target_site", request.TargetSite }
        };

        if (!string.IsNullOrEmpty(request.ClientIp))
        {
            form.Add("client_ip", request.ClientIp);
        }

        var content = new FormUrlEncodedContent(form);
        var response = await client.PostAsync("trusted", content);
        response.EnsureSuccessStatusCode();

        var ticketId = await response.Content.ReadAsStringAsync();
        
        // Tableau returns -1 if ticket generation failed
        if (ticketId == "-1")
        {
            throw new InvalidOperationException("Failed to generate trusted ticket. Ensure trusted authentication is enabled on the Tableau Server.");
        }

        var embedUrl = GenerateEmbedUrl("", ticketId);

        return new TrustedTicketResponse
        {
            TicketId = ticketId.Trim(),
            EmbedUrl = embedUrl
        };
    }

    public string GenerateEmbedUrl(string viewUrl, string? ticket = null)
    {
        var baseUrl = $"{_tableauOptions.Server}/trusted/{ticket ?? "{ticket}"}/t/{_authOptions.SiteContentUrl}/views/{viewUrl}";
        return baseUrl;
    }

    public string GenerateWorkbookEmbedUrl(string workbookUrl, string? ticket = null)
    {
        var baseUrl = $"{_tableauOptions.Server}/trusted/{ticket ?? "{ticket}"}/t/{_authOptions.SiteContentUrl}/workbooks/{workbookUrl}";
        return baseUrl;
    }
}

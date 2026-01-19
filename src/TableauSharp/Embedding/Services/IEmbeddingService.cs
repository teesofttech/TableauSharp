using TableauSharp.Embedding.Models;

namespace TableauSharp.Embedding.Services;

/// <summary>
/// Service for Tableau embedding capabilities
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Request a trusted ticket for embedding
    /// </summary>
    Task<TrustedTicketResponse> GetTrustedTicketAsync(TrustedTicketRequest request);

    /// <summary>
    /// Generate an embed URL for a view
    /// </summary>
    string GenerateEmbedUrl(string viewUrl, string? ticket = null);

    /// <summary>
    /// Generate an embed URL for a workbook
    /// </summary>
    string GenerateWorkbookEmbedUrl(string workbookUrl, string? ticket = null);
}

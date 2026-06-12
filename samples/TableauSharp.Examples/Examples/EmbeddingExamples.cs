using Microsoft.Extensions.Logging;
using TableauSharp.Auth.Service;
using TableauSharp.Embedding.Models;
using TableauSharp.Embedding.Services;

namespace TableauSharp.Examples.Examples;

// Demonstrates Tableau embedding via Trusted Tickets.
// Trusted Tickets are the legacy embedding mechanism for Tableau Server.
// Trusted authentication must be enabled on the Tableau Server and the
// caller's IP must be added to the trusted hosts list.
//
// For Tableau Cloud or newer Server versions consider Connected Apps (JWT) instead.
public class EmbeddingExamples(
    IAuthService authService,
    IEmbeddingService embeddingService,
    ILogger<EmbeddingExamples> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("--- Embedding Examples ---");
        Console.WriteLine();

        await authService.SignInWithPATAsync();

        await GetTrustedTicketAsync();
        GenerateEmbedUrls();
    }

    // A trusted ticket is a single-use token that lets a user view embedded content
    // without being prompted to log in. Redeem it within 3 minutes.
    private async Task GetTrustedTicketAsync()
    {
        Console.WriteLine("[1] Get a trusted ticket");
        try
        {
            var ticket = await embeddingService.GetTrustedTicketAsync(new TrustedTicketRequest
            {
                Username = "viewer@company.com",
                TargetSite = "your-site-name"
                // ClientIp is optional — only needed if IP restriction is enabled
            });

            Console.WriteLine($"    TicketId : {ticket.TicketId}");
            Console.WriteLine($"    EmbedUrl : {ticket.EmbedUrl}");
            Console.WriteLine();
            Console.WriteLine("    Append the view path to EmbedUrl to embed a specific view:");
            Console.WriteLine($"    {ticket.EmbedUrl}/views/MyWorkbook/MyView");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Trusted ticket request failed");
        }

        Console.WriteLine();
    }

    // URL helpers let you build embed URLs without making an HTTP call.
    // Pass a ticket obtained from GetTrustedTicketAsync, or omit it to get
    // a template URL with a {ticket} placeholder.
    private void GenerateEmbedUrls()
    {
        Console.WriteLine("[2] Generate embed URLs (no HTTP call)");

        var viewUrl = embeddingService.GenerateEmbedUrl("SalesReport/Overview");
        Console.WriteLine($"    View URL (no ticket) : {viewUrl}");

        var viewWithTicket = embeddingService.GenerateEmbedUrl("SalesReport/Overview", "abc123ticket");
        Console.WriteLine($"    View URL (with ticket): {viewWithTicket}");

        var workbookUrl = embeddingService.GenerateWorkbookEmbedUrl("SalesReport", "abc123ticket");
        Console.WriteLine($"    Workbook URL          : {workbookUrl}");

        Console.WriteLine();
    }
}

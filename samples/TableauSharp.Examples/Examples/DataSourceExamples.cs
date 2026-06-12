using Microsoft.Extensions.Logging;
using TableauSharp.Auth.Service;
using TableauSharp.DataSources.Services;

namespace TableauSharp.Examples.Examples;

// Demonstrates data source operations.
// RefreshAsync triggers an extract refresh job on Tableau Server.
// The job runs asynchronously — use Tableau's Jobs API to poll completion.
public class DataSourceExamples(
    IAuthService authService,
    IDataSourceService dataSourceService,
    ILogger<DataSourceExamples> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("--- Data Source Examples ---");
        Console.WriteLine();

        await authService.SignInWithPATAsync();

        await ListDataSourcesAsync();
        await RefreshFirstDataSourceAsync();
    }

    private async Task ListDataSourcesAsync()
    {
        Console.WriteLine("[1] List all data sources");
        try
        {
            var sources = await dataSourceService.GetAllAsync();
            foreach (var ds in sources)
            {
                var certified = ds.IsCertified ? " [certified]" : "";
                Console.WriteLine($"    {ds.Id}  {ds.Name,-35}  type={ds.Type ?? "unknown"}{certified}");
            }

            if (!sources.Any())
                Console.WriteLine("    No data sources found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "List data sources failed");
        }

        Console.WriteLine();
    }

    // Triggering a refresh is fire-and-forget from the SDK's perspective.
    // Tableau Server queues the job and runs it asynchronously.
    private async Task RefreshFirstDataSourceAsync()
    {
        Console.WriteLine("[2] Trigger extract refresh on first data source");
        try
        {
            var sources = await dataSourceService.GetAllAsync();
            var first = sources.FirstOrDefault();
            if (first is null) { Console.WriteLine("    No data sources."); return; }

            await dataSourceService.RefreshAsync(first.Id);
            Console.WriteLine($"    Refresh triggered for '{first.Name}'.");
            Console.WriteLine("    Check Tableau Server > Jobs to monitor completion.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Refresh failed");
        }

        Console.WriteLine();
    }
}

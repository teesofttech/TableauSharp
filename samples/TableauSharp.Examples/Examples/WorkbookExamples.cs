using Microsoft.Extensions.Logging;
using TableauSharp.Auth.Service;
using TableauSharp.Workbooks.Models;
using TableauSharp.Workbooks.Services;

namespace TableauSharp.Examples.Examples;

// Demonstrates workbook and view operations.
// Exporting views requires a Tableau Server licence that permits image/data export.
public class WorkbookExamples(
    IAuthService authService,
    IWorkbookService workbookService,
    IViewService viewService,
    ILogger<WorkbookExamples> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("--- Workbook Examples ---");
        Console.WriteLine();

        await authService.SignInWithPATAsync();

        await ListWorkbooksAsync();
        await ListViewsForFirstWorkbookAsync();
        await ExportViewAsPngAsync();
    }

    private async Task ListWorkbooksAsync()
    {
        Console.WriteLine("[1] List workbooks");
        try
        {
            var workbooks = await workbookService.GetAllAsync();
            foreach (var wb in workbooks)
                Console.WriteLine($"    {wb.Id}  {wb.Name,-40}  project={wb.ProjectId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "List workbooks failed");
        }

        Console.WriteLine();
    }

    private async Task ListViewsForFirstWorkbookAsync()
    {
        Console.WriteLine("[2] List views for the first workbook");
        try
        {
            var workbooks = await workbookService.GetAllAsync();
            var first = workbooks.FirstOrDefault();
            if (first is null)
            {
                Console.WriteLine("    No workbooks found.");
                return;
            }

            Console.WriteLine($"    Workbook: {first.Name}");
            var views = await viewService.GetViewsByWorkbookIdAsync(first.Id);
            foreach (var v in views)
                Console.WriteLine($"      View: {v.Id}  {v.Name,-30}  totalViews={v.TotalViews}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "List views failed");
        }

        Console.WriteLine();
    }

    // ExportViewAsync returns raw bytes which you can save to a file or stream to a browser.
    // Supported formats: PDF, PNG, CSV
    private async Task ExportViewAsPngAsync()
    {
        Console.WriteLine("[3] Export a view as PNG");
        try
        {
            var workbooks = await workbookService.GetAllAsync();
            var first = workbooks.FirstOrDefault();
            if (first is null) { Console.WriteLine("    No workbooks."); return; }

            var views = await viewService.GetViewsByWorkbookIdAsync(first.Id);
            var firstView = views.FirstOrDefault();
            if (firstView is null) { Console.WriteLine("    No views."); return; }

            var export = await viewService.ExportViewAsync(new ExportRequest
            {
                ViewId = firstView.Id,
                Format = "PNG"
            });

            var outputPath = Path.Combine(Path.GetTempPath(), export.FileName);
            await File.WriteAllBytesAsync(outputPath, export.FileContent);
            Console.WriteLine($"    Saved {export.FileContent.Length:N0} bytes → {outputPath}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export view failed");
        }

        Console.WriteLine();
    }
}

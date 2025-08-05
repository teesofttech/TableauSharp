using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public interface IViewService
{
    Task<IEnumerable<TableauView>> GetViewsByWorkbookIdAsync(string workbookId);
    Task<ExportResponse> ExportViewAsync(ExportRequest request);
}
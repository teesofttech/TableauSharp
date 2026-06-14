using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public interface IViewService
{
    Task<IEnumerable<TableauView>> GetViewsByWorkbookIdAsync(string workbookId, CancellationToken cancellationToken = default);
    Task<ExportResponse> ExportViewAsync(ExportRequest request, CancellationToken cancellationToken = default);
}

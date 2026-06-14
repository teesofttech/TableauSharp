using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public interface IWorkbookService
{
    Task<IEnumerable<TableauWorkbook>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TableauWorkbook> GetByIdAsync(string workbookId, CancellationToken cancellationToken = default);
    Task<TableauWorkbook> PublishAsync(WorkbookPublishRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string workbookId, CancellationToken cancellationToken = default);
}

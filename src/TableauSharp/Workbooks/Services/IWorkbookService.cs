using TableauSharp.Workbooks.Models;

namespace TableauSharp.Workbooks.Services;

public interface IWorkbookService
{
    Task<IEnumerable<TableauWorkbook>> GetAllAsync();
    Task<TableauWorkbook> GetByIdAsync(string workbookId);
    Task<TableauWorkbook> PublishAsync(WorkbookPublishRequest request);
    Task DeleteAsync(string workbookId);
}
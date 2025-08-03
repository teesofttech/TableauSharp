using TableauSharp.Projects.Models;

namespace TableauSharp.Projects.Services;

public interface IProjectService
{
    Task<IEnumerable<TableauProject>> GetAllAsync();
    Task<TableauProject> GetByIdAsync(string projectId);
    Task<TableauProject> CreateAsync(ProjectCreateRequest request);
    Task<TableauProject> UpdateAsync(string projectId, ProjectUpdateRequest request);
    Task DeleteAsync(string projectId);
}
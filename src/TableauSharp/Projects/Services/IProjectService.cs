using TableauSharp.Projects.Models;

namespace TableauSharp.Projects.Services;

public interface IProjectService
{
    Task<IEnumerable<TableauProject>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TableauProject> GetByIdAsync(string projectId, CancellationToken cancellationToken = default);
    Task<TableauProject> CreateAsync(ProjectCreateRequest request, CancellationToken cancellationToken = default);
    Task<TableauProject> UpdateAsync(string projectId, ProjectUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string projectId, CancellationToken cancellationToken = default);
}

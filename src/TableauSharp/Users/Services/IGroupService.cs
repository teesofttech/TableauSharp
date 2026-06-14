using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public interface IGroupService
{
    Task<IEnumerable<TableauGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TableauGroup> GetByIdAsync(string groupId, CancellationToken cancellationToken = default);
    Task<TableauGroup> CreateAsync(GroupCreateRequest request, CancellationToken cancellationToken = default);
    Task AddUserToGroupAsync(string groupId, string userId, CancellationToken cancellationToken = default);
    Task RemoveUserFromGroupAsync(string groupId, string userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(string groupId, CancellationToken cancellationToken = default);
}

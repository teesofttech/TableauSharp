using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public interface IGroupService
{
    Task<IEnumerable<TableauGroup>> GetAllAsync();
    Task<TableauGroup> GetByIdAsync(string groupId);
    Task<TableauGroup> CreateAsync(GroupCreateRequest request);
    Task AddUserToGroupAsync(string groupId, string userId);
    Task RemoveUserFromGroupAsync(string groupId, string userId);
    Task DeleteAsync(string groupId);
}
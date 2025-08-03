using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public interface IUserService
{
    Task<IEnumerable<TableauUser>> GetAllAsync();
    Task<TableauUser> GetByIdAsync(string userId);
    Task<TableauUser> CreateAsync(UserCreateRequest request);
    Task<TableauUser> UpdateAsync(string userId, UserUpdateRequest request);
    Task DeleteAsync(string userId);
}
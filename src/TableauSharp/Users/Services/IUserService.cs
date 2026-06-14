using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public interface IUserService
{
    Task<IEnumerable<TableauUser>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TableauUser> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<TableauUser> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task<TableauUser> UpdateAsync(string userId, UserUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userId, CancellationToken cancellationToken = default);
}

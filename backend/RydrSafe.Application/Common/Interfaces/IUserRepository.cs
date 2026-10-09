using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Whether another account already holds this address. Excludes <paramref name="excludeUserId"/>
    /// so a user re-saving their profile without changing their email is not told it is taken.
    /// </summary>
    Task<bool> EmailTakenAsync(string email, Guid excludeUserId);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}

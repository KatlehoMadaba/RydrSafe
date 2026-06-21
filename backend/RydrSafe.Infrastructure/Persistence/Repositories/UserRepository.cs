using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id) =>
        await db.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await db.Users.ToListAsync();

    public async Task AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is not null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }
}

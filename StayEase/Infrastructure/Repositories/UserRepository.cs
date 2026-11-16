using Microsoft.EntityFrameworkCore;
using StayEase.Domain;
using StayEase.Infrastructure.Data;

namespace StayEase.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
}

public class UserRepository(IDbContextFactory<StayEaseDbContext> factory) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}

using StayEase.Domain;
using StayEase.Infrastructure.Repositories;

namespace StayEase.Services;

public class AuthService(IUserRepository users)
{
    public async Task<Result<User>> RegisterAsync(string? name, string? email, string? password)
    {
        name = (name ?? "").Trim();
        email = (email ?? "").Trim().ToLowerInvariant();

        if (name.Length < 2) return Result<User>.Fail("Name must be at least 2 characters.");
        if (email.Length < 5 || !email.Contains('@')) return Result<User>.Fail("Enter a valid email address.");
        if (string.IsNullOrEmpty(password) || password.Length < 6) return Result<User>.Fail("Password must be at least 6 characters.");
        if (await users.GetByEmailAsync(email) is not null) return Result<User>.Fail("This email is already registered.");

        var user = new User
        {
            Name = name, Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Role = UserRole.Customer, // регистрация всегда создаёт обычного пользователя
        };
        await users.AddAsync(user);
        return Result<User>.Ok(user);
    }

    public async Task<Result<User>> LoginAsync(string? email, string? password)
    {
        var user = await users.GetByEmailAsync((email ?? "").Trim().ToLowerInvariant());
        if (user is null || string.IsNullOrEmpty(password) || !PasswordHasher.Verify(password, user.PasswordHash))
            return Result<User>.Fail("Invalid email or password.");
        return Result<User>.Ok(user);
    }
}

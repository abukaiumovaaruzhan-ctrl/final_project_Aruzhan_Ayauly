using StayEase.Domain;
using StayEase.Infrastructure.Data;
using StayEase.Services;

namespace StayEase.Tests;

public class AuthServiceTests
{
    // 13
    [Fact]
    public async Task Registration_creates_a_customer_with_hashed_password()
    {
        using var env = new TestEnv();

        var result = await env.Auth.RegisterAsync("Aruzhan", "Aruzhan@Example.com", "secret1");

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(UserRole.Customer, result.Value!.Role);
        Assert.Equal("aruzhan@example.com", result.Value.Email);
        Assert.DoesNotContain("secret1", result.Value.PasswordHash);
    }

    [Fact]
    public async Task Registration_rejects_duplicate_email_and_weak_password()
    {
        using var env = new TestEnv();
        Assert.False((await env.Auth.RegisterAsync("Demo", DbInitializer.DemoEmail, "secret1")).IsSuccess);
        Assert.False((await env.Auth.RegisterAsync("Ok Name", "new@example.com", "123")).IsSuccess);
    }

    // 14
    [Fact]
    public async Task Login_with_wrong_password_fails()
    {
        using var env = new TestEnv();
        var result = await env.Auth.LoginAsync(DbInitializer.DemoEmail, "wrong-password");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Login_with_correct_password_succeeds()
    {
        using var env = new TestEnv();
        var result = await env.Auth.LoginAsync(DbInitializer.AdminEmail, DbInitializer.AdminPassword);
        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.Admin, result.Value!.Role);
    }

    [Fact]
    public void Password_hasher_verifies_only_the_right_password()
    {
        var hash = PasswordHasher.Hash("abc123");
        Assert.True(PasswordHasher.Verify("abc123", hash));
        Assert.False(PasswordHasher.Verify("abc124", hash));
    }
}

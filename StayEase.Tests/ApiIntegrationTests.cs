using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using StayEase.Infrastructure.Data;

namespace StayEase.Tests;

/// <summary>Запускает приложение целиком (HTTP + авторизация + настоящая SQLite) на временной базе.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"stayease-api-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = $"Data Source={DbPath}" }));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try { File.Delete(DbPath); } catch { }
    }

    public HttpClient NewClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
}

public class ApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public ApiIntegrationTests(ApiFactory factory) => _factory = factory;

    private static async Task<HttpResponseMessage> Login(HttpClient c, string email, string password) =>
        await c.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["email"] = email, ["password"] = password }));

    [Fact]
    public async Task Hotels_list_is_public_and_comes_from_the_database()
    {
        var client = _factory.NewClient();
        var response = await client.GetAsync("/api/hotels");
        var hotels = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(hotels!.Count >= 5);
    }

    [Fact]
    public async Task Anonymous_user_cannot_use_protected_endpoints()
    {
        var client = _factory.NewClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/stats")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/bookings/mine")).StatusCode);
    }

    // 12
    [Fact]
    public async Task Customer_is_forbidden_from_admin_api()
    {
        var client = _factory.NewClient();
        var login = await Login(client, DbInitializer.DemoEmail, DbInitializer.DemoPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var create = await client.PostAsJsonAsync("/api/hotels",
            new { name = "Hack Inn", city = "X", address = "", description = "", stars = 3, rating = 5.0 });
        var stats = await client.GetAsync("/api/stats");
        var all = await client.GetAsync("/api/bookings");

        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, stats.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, all.StatusCode);
    }

    [Fact]
    public async Task Admin_can_open_stats_and_create_hotel()
    {
        var client = _factory.NewClient();
        await Login(client, DbInitializer.AdminEmail, DbInitializer.AdminPassword);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/stats")).StatusCode);
        var create = await client.PostAsJsonAsync("/api/hotels",
            new { name = "API Inn", city = "Astana", address = "Street 1", description = "d", stars = 4, rating = 8.0 });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
    }

    [Fact]
    public async Task Customer_can_book_and_then_cancel_through_api()
    {
        var client = _factory.NewClient();
        await Login(client, DbInitializer.DemoEmail, DbInitializer.DemoPassword);
        var checkIn = DateOnly.FromDateTime(DateTime.Today).AddDays(200);

        var create = await client.PostAsJsonAsync("/api/bookings",
            new { roomId = 1, checkIn = checkIn.ToString("yyyy-MM-dd"), checkOut = checkIn.AddDays(3).ToString("yyyy-MM-dd"), guests = 2 });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var booking = await create.Content.ReadFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>();
        var id = booking!["id"].GetInt32();

        var cancel = await client.PostAsync($"/api/bookings/{id}/cancel", null);
        var again = await client.PostAsync($"/api/bookings/{id}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
    }

    [Fact]
    public async Task Registration_signs_the_user_in_and_wrong_login_is_rejected()
    {
        var client = _factory.NewClient();
        var email = $"user{Guid.NewGuid():N}@example.com";

        var register = await client.PostAsync("/auth/register", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["name"] = "New User", ["email"] = email, ["password"] = "secret1", ["confirm"] = "secret1" }));
        Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/bookings/mine")).StatusCode);

        var bad = await Login(_factory.NewClient(), email, "not-the-password");
        Assert.StartsWith("/login?error=", bad.Headers.Location!.OriginalString);
    }
}

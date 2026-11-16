using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StayEase.Api;
using StayEase.Auth;
using StayEase.Components;
using StayEase.Infrastructure.Data;
using StayEase.Infrastructure.Repositories;
using StayEase.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---- База данных (SQLite) ----
var defaultDb = "Data Source=" + Path.Combine(builder.Environment.ContentRootPath, "stayease.db");
builder.Services.AddDbContextFactory<StayEaseDbContext>((sp, options) =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default");
    options.UseSqlite(string.IsNullOrWhiteSpace(cs) ? defaultDb : cs);
});

// ---- Авторизация (cookie) ----
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.Cookie.Name = "stayease.auth";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        // Для /api возвращаем 401/403, а не перенаправление на страницу входа.
        o.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            else ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
        o.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorizationBuilder().AddPolicy("Admin", p => p.RequireRole("Admin"));

// ---- Сервисы ----
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<BookingNotifier>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<HotelService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<BookingState>();
builder.Services.AddScoped<ToastService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StayEaseDbContext>>();
    using var db = factory.CreateDbContext();
    DbInitializer.Initialize(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapAuthEndpoints();
app.MapApiEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

// Нужен интеграционным тестам (WebApplicationFactory<Program>).
public partial class Program { }

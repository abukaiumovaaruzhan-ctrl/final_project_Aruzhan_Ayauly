using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StayEase.Domain;
using StayEase.Services;

namespace StayEase.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", async (HttpContext ctx, AuthService auth) =>
        {
            if (!ctx.Request.HasFormContentType) return Results.BadRequest();
            var form = await ctx.Request.ReadFormAsync();
            var result = await auth.LoginAsync(form["email"].ToString(), form["password"].ToString());
            if (!result.IsSuccess) return Results.Redirect("/login?error=" + Uri.EscapeDataString(result.Error!));

            await SignIn(ctx, result.Value!);
            return Results.Redirect(result.Value!.Role == UserRole.Admin ? "/admin" : "/");
        });

        app.MapPost("/auth/register", async (HttpContext ctx, AuthService auth) =>
        {
            if (!ctx.Request.HasFormContentType) return Results.BadRequest();
            var form = await ctx.Request.ReadFormAsync();
            if (form["password"].ToString() != form["confirm"].ToString())
                return Results.Redirect("/register?error=" + Uri.EscapeDataString("Passwords do not match."));

            var result = await auth.RegisterAsync(form["name"].ToString(), form["email"].ToString(), form["password"].ToString());
            if (!result.IsSuccess) return Results.Redirect("/register?error=" + Uri.EscapeDataString(result.Error!));

            await SignIn(ctx, result.Value!);
            return Results.Redirect("/");
        });

        app.MapPost("/auth/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });
    }

    private static Task SignIn(HttpContext ctx, User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}

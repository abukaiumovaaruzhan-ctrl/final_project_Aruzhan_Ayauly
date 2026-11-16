using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace StayEase.Services;

public record CurrentUser(int Id, string Name, bool IsAdmin)
{
    public static CurrentUser? From(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return null;
        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(id, out var userId)) return null;
        return new CurrentUser(userId, principal.Identity?.Name ?? "", principal.IsInRole("Admin"));
    }
}

/// <summary>Текущий пользователь для Blazor-компонентов.</summary>
public class UserContext(AuthenticationStateProvider auth)
{
    public async Task<CurrentUser?> GetAsync() =>
        CurrentUser.From((await auth.GetAuthenticationStateAsync()).User);
}

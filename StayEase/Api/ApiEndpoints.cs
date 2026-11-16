using StayEase.Domain;
using StayEase.Services;

namespace StayEase.Api;

public static class ApiEndpoints
{
    private static CurrentUser Actor(HttpContext ctx) => CurrentUser.From(ctx.User)!;

    private static IResult Respond<T>(Result<T> r, Func<T, object> map) =>
        r.IsSuccess ? Results.Ok(map(r.Value!)) : Results.BadRequest(new { error = r.Error });

    private static IResult Respond(Result r) =>
        r.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = r.Error });

    public static void MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // ---- публичные ----
        api.MapGet("/hotels", async (HotelService s) =>
            (await s.GetAllAsync()).Select(h => h.ToDto()).ToList());

        api.MapGet("/hotels/{id:int}", async (int id, HotelService s) =>
            await s.GetAsync(id) is { } h ? Results.Ok(h.ToDto()) : Results.NotFound(new { error = "Hotel not found." }));

        // ---- только для авторизованных ----
        api.MapGet("/bookings/mine", async (HttpContext ctx, BookingService s) =>
            (await s.GetMineAsync(Actor(ctx))).Select(b => b.ToDto()).ToList())
            .RequireAuthorization();

        api.MapPost("/bookings", async (CreateBookingRequest r, HttpContext ctx, BookingService s) =>
            Respond(await s.CreateAsync(Actor(ctx), r.RoomId, r.CheckIn, r.CheckOut, r.Guests), b => b.ToDto()))
            .RequireAuthorization();

        api.MapPost("/bookings/{id:int}/cancel", async (int id, HttpContext ctx, BookingService s) =>
            Respond(await s.CancelAsync(Actor(ctx), id)))
            .RequireAuthorization();

        // ---- только для администратора ----
        api.MapPost("/hotels", async (HotelRequest r, HttpContext ctx, HotelService s) =>
            Respond(await s.AddAsync(Actor(ctx), r.ToInput()), h => h.ToDto()))
            .RequireAuthorization("Admin");

        api.MapPut("/hotels/{id:int}", async (int id, HotelRequest r, HttpContext ctx, HotelService s) =>
            Respond(await s.UpdateAsync(Actor(ctx), id, r.ToInput()), h => h.ToDto()))
            .RequireAuthorization("Admin");

        api.MapDelete("/hotels/{id:int}", async (int id, HttpContext ctx, HotelService s) =>
            Respond(await s.DeleteAsync(Actor(ctx), id)))
            .RequireAuthorization("Admin");

        api.MapPost("/rooms", async (RoomRequest r, HttpContext ctx, HotelService s) =>
            Respond(await s.AddRoomAsync(Actor(ctx), r.ToInput()), room => room.ToDto()))
            .RequireAuthorization("Admin");

        api.MapDelete("/rooms/{id:int}", async (int id, HttpContext ctx, HotelService s) =>
            Respond(await s.DeleteRoomAsync(Actor(ctx), id)))
            .RequireAuthorization("Admin");

        api.MapGet("/bookings", async (HttpContext ctx, BookingService s) =>
            (await s.GetAllAsync(Actor(ctx))).Select(b => b.ToDto()).ToList())
            .RequireAuthorization("Admin");

        api.MapPut("/bookings/{id:int}/status", async (int id, StatusRequest r, HttpContext ctx, BookingService s) =>
            Respond(await s.SetStatusAsync(Actor(ctx), id, r.Status)))
            .RequireAuthorization("Admin");

        api.MapGet("/stats", async (HttpContext ctx, StatsService s) =>
            Respond(await s.GetAsync(Actor(ctx)), x => x))
            .RequireAuthorization("Admin");
    }
}

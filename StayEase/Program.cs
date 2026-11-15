using System.Text.Json.Serialization;
using StayEase.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BookingService>();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseDefaultFiles();   // index.html по умолчанию
app.UseStaticFiles();    // всё из папки wwwroot

// Превращает DomainException в понятный ответ 400 для сайта.
static IResult Safe(Func<IResult> action)
{
    try { return action(); }
    catch (DomainException ex) { return Results.BadRequest(new { error = ex.Message }); }
}

var api = app.MapGroup("/api");

api.MapGet("/hotels", (BookingService s) => s.GetHotels());
api.MapGet("/hotels/{id:int}", (int id, BookingService s) =>
    s.GetHotel(id) is { } h ? Results.Ok(h) : Results.NotFound(new { error = "Hotel not found." }));
api.MapPost("/hotels", (NewHotel n, BookingService s) => Safe(() => Results.Ok(s.AddHotel(n))));
api.MapDelete("/hotels/{id:int}", (int id, BookingService s) => Safe(() => { s.DeleteHotel(id); return Results.NoContent(); }));

api.MapPost("/rooms", (NewRoom n, BookingService s) => Safe(() => Results.Ok(s.AddRoom(n))));
api.MapDelete("/rooms/{id:int}", (int id, BookingService s) => Safe(() => { s.DeleteRoom(id); return Results.NoContent(); }));

api.MapGet("/bookings", (int? customerId, BookingService s) => s.GetBookings(customerId));
api.MapPost("/bookings", (NewBooking n, BookingService s) => Safe(() => Results.Ok(s.Book(n))));
api.MapPut("/bookings/{id:int}/status", (int id, StatusChange c, BookingService s) =>
    Safe(() => Results.Ok(s.SetStatus(id, c.Status))));

api.MapGet("/stats", (BookingService s) => s.GetStats());

app.Run();

// Нужен, чтобы тесты могли запускать приложение.
public partial class Program;

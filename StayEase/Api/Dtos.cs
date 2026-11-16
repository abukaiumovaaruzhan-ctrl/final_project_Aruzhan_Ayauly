using StayEase.Domain;

namespace StayEase.Api;

public record RoomDto(int Id, int HotelId, string Number, string RoomType, decimal Price, int Capacity);

public record HotelDto(int Id, string Name, string City, string Address, string Description,
    int Stars, double Rating, string ImageUrl, decimal? FromPrice, IReadOnlyList<RoomDto> Rooms);

public record BookingDto(int Id, int UserId, string Customer, string Hotel, string Room,
    DateOnly CheckIn, DateOnly CheckOut, int Guests, decimal TotalPrice, string Status);

public record CreateBookingRequest(int RoomId, DateOnly CheckIn, DateOnly CheckOut, int Guests);
public record StatusRequest(BookingStatus Status);
public record RoomRequest(int HotelId, string Number, RoomType RoomType, decimal Price, int Capacity);
public record HotelRequest(string Name, string City, string Address, string Description, int Stars, double Rating, string? ImageUrl);

public static class Mapping
{
    public static RoomDto ToDto(this Room r) =>
        new(r.Id, r.HotelId, r.Number, r.RoomType.ToString(), r.Price, r.Capacity);

    public static HotelDto ToDto(this Hotel h) =>
        new(h.Id, h.Name, h.City, h.Address, h.Description, h.Stars, h.Rating, h.ImageUrl,
            HotelQuery.FromPrice(h), h.Rooms.Select(r => r.ToDto()).ToList());

    public static BookingDto ToDto(this Booking b) =>
        new(b.Id, b.UserId, b.User?.Name ?? "—", b.Room?.Hotel?.Name ?? "—", b.Room?.Number ?? "—",
            b.CheckIn, b.CheckOut, b.Guests, b.TotalPrice, b.Status.ToString());

    public static HotelInput ToInput(this HotelRequest r) =>
        new(r.Name, r.City, r.Address, r.Description, r.Stars, r.Rating, r.ImageUrl);

    public static RoomInput ToInput(this RoomRequest r) =>
        new(r.HotelId, r.Number, r.RoomType, r.Price, r.Capacity);
}

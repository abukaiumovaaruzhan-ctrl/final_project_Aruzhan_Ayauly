namespace StayEase.Domain;

public enum RoomType { Standard, Deluxe, Suite }

public enum BookingStatus { Pending, Confirmed, Cancelled }

// Неизменяемые типы данных (record) — основа функционального стиля в C#.
public record Hotel(int Id, string Name, string City, int Stars, string Description, string Image);

public record Room(int Id, int HotelId, string Number, RoomType Type, decimal Price, int Capacity);

public record Booking(
    int Id, int CustomerId, int RoomId,
    DateOnly CheckIn, DateOnly CheckOut,
    decimal Total, BookingStatus Status);

// Данные, которые приходят от клиента
public record NewHotel(string Name, string City, int Stars, string? Description, string? Image);
public record NewRoom(int HotelId, string Number, RoomType Type, decimal Price, int Capacity);
public record NewBooking(int RoomId, DateOnly CheckIn, DateOnly CheckOut);
public record StatusChange(BookingStatus Status);

// Данные, которые уходят клиенту
public record HotelSummary(Hotel Hotel, decimal? FromPrice);
public record HotelDetails(Hotel Hotel, IReadOnlyList<Room> Rooms);
public record BookingView(
    int Id, int CustomerId, string Hotel, string Room,
    DateOnly CheckIn, DateOnly CheckOut, int Nights,
    decimal Total, BookingStatus Status);
public record Stats(decimal Revenue, int Bookings, int Hotels, int Rooms);

public class DomainException(string message) : Exception(message);

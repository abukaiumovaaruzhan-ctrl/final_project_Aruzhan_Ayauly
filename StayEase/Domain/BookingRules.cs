namespace StayEase.Domain;

/// <summary>Чистая проверка бронирования. Возвращает текст ошибки или null, если всё в порядке.</summary>
public static class BookingRules
{
    public static string? Validate(
        DateOnly checkIn, DateOnly checkOut, int guests,
        Room room, DateOnly today, IEnumerable<Booking> existingForRoom)
    {
        if (checkIn < today) return "Check-in date cannot be in the past.";
        if (checkOut <= checkIn) return "Check-out must be after check-in.";
        if (guests < 1) return "At least one guest is required.";
        if (guests > room.Capacity) return $"This room fits up to {room.Capacity} guests.";

        var clash = existingForRoom.Any(b =>
            b.RoomId == room.Id &&
            b.Status != BookingStatus.Cancelled &&
            Pricing.Overlaps(checkIn, checkOut, b.CheckIn, b.CheckOut));

        return clash ? "This room is already booked for those dates." : null;
    }
}

public record HotelInput(string Name, string City, string Address, string Description, int Stars, double Rating, string? ImageUrl);
public record RoomInput(int HotelId, string Number, RoomType RoomType, decimal Price, int Capacity);

public static class CatalogRules
{
    public static string? Validate(HotelInput i)
    {
        if (string.IsNullOrWhiteSpace(i.Name) || string.IsNullOrWhiteSpace(i.City)) return "Name and city are required.";
        if (i.Stars is < 1 or > 5) return "Stars must be between 1 and 5.";
        if (i.Rating is < 0 or > 10) return "Rating must be between 0 and 10.";
        return null;
    }

    public static string? Validate(RoomInput i)
    {
        if (string.IsNullOrWhiteSpace(i.Number)) return "Room number is required.";
        if (i.Price <= 0) return "Price must be positive.";
        if (i.Capacity <= 0) return "Capacity must be positive.";
        return null;
    }
}

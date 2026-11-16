namespace StayEase.Domain;

public enum UserRole { Customer, Admin }
public enum RoomType { Standard, Deluxe, Suite }
public enum BookingStatus { Pending, Confirmed, Cancelled }

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; }
}

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public int Stars { get; set; }
    public double Rating { get; set; }
    public string ImageUrl { get; set; } = "/hero.jpg";
    public List<Room> Rooms { get; set; } = [];
}

public class Room
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;
    public string Number { get; set; } = "";
    public RoomType RoomType { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
}

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Guests { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
}

public class DomainException(string message) : Exception(message);

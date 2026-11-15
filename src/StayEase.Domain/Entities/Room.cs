namespace StayEase.Domain.Entities;

public class Room
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public string RoomNumber { get; set; } = "";
    public string Type { get; set; } = "Standard";
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; } = true;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

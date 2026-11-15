namespace StayEase.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";

    public decimal CalculateTotalPrice(decimal pricePerNight)
    {
        var nights = CheckOut.DayNumber - CheckIn.DayNumber;
        if (nights <= 0) throw new ArgumentException("Check-out must be after check-in.");
        if (pricePerNight < 0) throw new ArgumentOutOfRangeException(nameof(pricePerNight));
        return nights * pricePerNight;
    }
}

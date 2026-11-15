namespace StayEase.Domain.Entities;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public double Rating { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Hotel name is required.");
        if (string.IsNullOrWhiteSpace(Location)) throw new ArgumentException("Location is required.");
        if (Rating is < 0 or > 5) throw new ArgumentOutOfRangeException(nameof(Rating), "Rating must be between 0 and 5.");
    }
}

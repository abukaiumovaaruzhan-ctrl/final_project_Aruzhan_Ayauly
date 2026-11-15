using StayEase.Domain.Entities;
using Xunit;

namespace StayEase.Tests;

public class HotelTests
{
    [Fact]
    public void Validate_Rejects_Rating_Above_Five()
    {
        var hotel = new Hotel { Name = "Test hotel", Location = "Astana", Rating = 5.1 };
        Assert.Throws<ArgumentOutOfRangeException>(() => hotel.Validate());
    }

    [Fact]
    public void Validate_Accepts_Valid_Hotel()
    {
        var hotel = new Hotel { Name = "Test hotel", Location = "Astana", Rating = 4.5 };
        hotel.Validate();
    }
}

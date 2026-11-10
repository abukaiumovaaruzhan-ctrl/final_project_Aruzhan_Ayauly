using StayEase.Domain.Entities;

namespace StayEase.Tests;

public class DomainSmokeTests
{
    [Fact]
    public void Hotel_CanBeCreated()
    {
        var hotel = new Hotel { Name = "StayEase Demo", City = "Astana" };

        Assert.Equal("StayEase Demo", hotel.Name);
        Assert.Equal("Astana", hotel.City);
    }
}

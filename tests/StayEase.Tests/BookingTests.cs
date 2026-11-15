using StayEase.Domain.Entities;
using Xunit;

namespace StayEase.Tests;

public class BookingTests
{
    [Fact]
    public void CalculateTotalPrice_Returns_Nights_Multiplied_By_Rate()
    {
        var booking = new Booking { CheckIn = new DateOnly(2026, 11, 15), CheckOut = new DateOnly(2026, 11, 18) };
        Assert.Equal(300m, booking.CalculateTotalPrice(100m));
    }

    [Fact]
    public void CalculateTotalPrice_Rejects_Invalid_Date_Range()
    {
        var booking = new Booking { CheckIn = new DateOnly(2026, 11, 18), CheckOut = new DateOnly(2026, 11, 18) };
        Assert.Throws<ArgumentException>(() => booking.CalculateTotalPrice(100m));
    }
}

using StayEase.Domain;

namespace StayEase.Tests;

public class PricingTests
{
    [Fact]
    public void Charges_price_times_nights() =>
        Assert.Equal(300m, Pricing.CalculatePrice(100, new(2026, 1, 1), new(2026, 1, 4)));

    [Fact]
    public void Gives_10_percent_off_for_7_nights() =>
        Assert.Equal(630m, Pricing.CalculatePrice(100, new(2026, 1, 1), new(2026, 1, 8)));

    [Fact]
    public void Breakdown_matches_the_example_from_the_spec()
    {
        var b = Pricing.Breakdown(35000, new(2026, 11, 20), new(2026, 11, 27));
        Assert.Equal(7, b.Nights);
        Assert.Equal(245000m, b.Subtotal);
        Assert.Equal(24500m, b.Discount);
        Assert.Equal(220500m, b.Total);
    }

    [Fact]
    public void Six_nights_have_no_discount() =>
        Assert.Equal(0m, Pricing.Breakdown(100, new(2026, 1, 1), new(2026, 1, 7)).Discount);

    [Fact]
    public void Rejects_zero_nights() =>
        Assert.Throws<DomainException>(() => Pricing.CalculatePrice(100, new(2026, 1, 5), new(2026, 1, 5)));

    [Fact]
    public void Checkout_day_is_free_for_next_guest() =>
        Assert.False(Pricing.Overlaps(new(2026, 1, 1), new(2026, 1, 3), new(2026, 1, 3), new(2026, 1, 5)));

    [Fact]
    public void Hotel_filter_by_city_stars_and_price()
    {
        var cheap = new Hotel { Name = "A", City = "Almaty", Stars = 3, Rating = 8, Rooms = [new Room { Price = 20000 }] };
        var fancy = new Hotel { Name = "B", City = "Astana", Stars = 5, Rating = 9.5, Rooms = [new Room { Price = 90000 }] };
        var all = new[] { cheap, fancy };

        Assert.Single(HotelQuery.Apply(all, "astana", new()));
        Assert.Single(HotelQuery.Apply(all, null, new(MinStars: 4)));
        Assert.Single(HotelQuery.Apply(all, null, new(MaxPrice: 50000)));
        Assert.Single(HotelQuery.Apply(all, null, new(MinRating: 9)));
        Assert.Equal(2, HotelQuery.Apply(all, "", new()).Count());
    }
}

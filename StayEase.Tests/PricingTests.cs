using Xunit;
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
    public void Rejects_zero_nights() =>
        Assert.Throws<DomainException>(() =>
            Pricing.CalculatePrice(100, new(2026, 1, 5), new(2026, 1, 5)));

    [Fact]
    public void Checkout_day_is_free_for_next_guest() =>
        Assert.False(Pricing.Overlaps(new(2026, 1, 1), new(2026, 1, 3), new(2026, 1, 3), new(2026, 1, 5)));
}

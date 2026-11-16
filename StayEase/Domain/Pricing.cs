namespace StayEase.Domain;

public record PriceBreakdown(int Nights, decimal PricePerNight, decimal Subtotal, decimal Discount, decimal Total);

/// <summary>Чистые функции: результат зависит только от аргументов, без побочных эффектов.</summary>
public static class Pricing
{
    public const int WeeklyDiscountFromNights = 7;
    public const decimal WeeklyDiscountRate = 0.10m;

    public static int Nights(DateOnly checkIn, DateOnly checkOut) => checkOut.DayNumber - checkIn.DayNumber;

    public static PriceBreakdown Breakdown(decimal pricePerNight, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = Nights(checkIn, checkOut);
        if (nights <= 0)
            throw new DomainException("Check-out must be after check-in.");

        var subtotal = pricePerNight * nights;
        var discount = nights >= WeeklyDiscountFromNights ? Math.Round(subtotal * WeeklyDiscountRate, 2) : 0m;
        return new PriceBreakdown(nights, pricePerNight, subtotal, discount, subtotal - discount);
    }

    public static decimal CalculatePrice(decimal pricePerNight, DateOnly checkIn, DateOnly checkOut) =>
        Breakdown(pricePerNight, checkIn, checkOut).Total;

    /// <summary>Пересекаются ли периоды (день выезда одного гостя свободен для следующего).</summary>
    public static bool Overlaps(DateOnly aIn, DateOnly aOut, DateOnly bIn, DateOnly bOut) => aIn < bOut && bIn < aOut;
}

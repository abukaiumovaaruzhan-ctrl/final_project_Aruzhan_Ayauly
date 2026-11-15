namespace StayEase.Domain;

/// <summary>
/// Чистые функции: результат зависит только от аргументов, побочных эффектов нет.
/// </summary>
public static class Pricing
{
    public const int WeeklyDiscountFromNights = 7;
    public const decimal WeeklyDiscountFactor = 0.9m;

    public static int Nights(DateOnly checkIn, DateOnly checkOut) =>
        checkOut.DayNumber - checkIn.DayNumber;

    /// <summary>Цена × ночи, скидка 10% от 7 ночей.</summary>
    public static decimal CalculatePrice(decimal pricePerNight, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = Nights(checkIn, checkOut);
        if (nights <= 0)
            throw new DomainException("Check-out must be after check-in.");

        var total = pricePerNight * nights;
        if (nights >= WeeklyDiscountFromNights)
            total *= WeeklyDiscountFactor;

        return Math.Round(total, 2);
    }

    /// <summary>Пересекаются ли два периода проживания (день выезда свободен).</summary>
    public static bool Overlaps(DateOnly aIn, DateOnly aOut, DateOnly bIn, DateOnly bOut) =>
        aIn < bOut && bIn < aOut;
}

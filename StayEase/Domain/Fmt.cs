using System.Globalization;

namespace StayEase.Domain;

public static class Fmt
{
    public static string Money(decimal v) =>
        Math.Round(v, 0).ToString("N0", CultureInfo.InvariantCulture).Replace(',', '\u00A0') + " ₸";

    public static string Date(DateOnly d) => d.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
}

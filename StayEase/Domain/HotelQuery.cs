namespace StayEase.Domain;

public record HotelFilterCriteria(decimal? MinPrice = null, decimal? MaxPrice = null, int MinStars = 0, double MinRating = 0);

/// <summary>Поиск и фильтрация отелей — чистая функция над списком.</summary>
public static class HotelQuery
{
    public static decimal? FromPrice(Hotel h) => h.Rooms.Count > 0 ? h.Rooms.Min(r => r.Price) : (decimal?)null;

    public static IEnumerable<Hotel> Apply(IEnumerable<Hotel> hotels, string? query, HotelFilterCriteria f)
    {
        var q = (query ?? "").Trim();
        return hotels
            .Where(h => q.Length == 0 ||
                        h.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        h.City.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Where(h => h.Stars >= f.MinStars)
            .Where(h => h.Rating >= f.MinRating)
            .Where(h =>
            {
                if (f.MinPrice is null && f.MaxPrice is null) return true;
                var price = FromPrice(h);
                if (price is null) return false;
                return (f.MinPrice is null || price >= f.MinPrice) && (f.MaxPrice is null || price <= f.MaxPrice);
            });
    }
}

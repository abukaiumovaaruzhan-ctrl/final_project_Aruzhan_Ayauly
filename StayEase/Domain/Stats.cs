namespace StayEase.Domain;

public record LabeledValue(string Label, decimal Value);

public record DashboardStats(
    decimal Revenue, int TotalBookings, int Hotels, int Rooms,
    IReadOnlyList<LabeledValue> RevenueByHotel,
    IReadOnlyList<LabeledValue> BookingsByStatus,
    IReadOnlyList<LabeledValue> PopularHotels,
    IReadOnlyList<LabeledValue> Occupancy);

public static class StatsCalculator
{
    public const int OccupancyWindowDays = 30;

    public static DashboardStats Compute(IReadOnlyList<Hotel> hotels, IReadOnlyList<Booking> bookings, DateOnly today)
    {
        var active = bookings.Where(b => b.Status != BookingStatus.Cancelled).ToList();
        var windowEnd = today.AddDays(OccupancyWindowDays);

        var revenueByHotel = hotels
            .Select(h => new LabeledValue(h.Name, active.Where(b => b.Room.HotelId == h.Id).Sum(b => b.TotalPrice)))
            .OrderByDescending(x => x.Value).ToList();

        var popular = hotels
            .Select(h => new LabeledValue(h.Name, active.Count(b => b.Room.HotelId == h.Id)))
            .OrderByDescending(x => x.Value).ToList();

        var byStatus = Enum.GetValues<BookingStatus>()
            .Select(s => new LabeledValue(s.ToString(), bookings.Count(b => b.Status == s)))
            .ToList();

        var occupancy = hotels.Select(h =>
        {
            var capacityNights = h.Rooms.Count * OccupancyWindowDays;
            var booked = active.Where(b => b.Room.HotelId == h.Id).Sum(b =>
            {
                var from = b.CheckIn > today ? b.CheckIn : today;
                var to = b.CheckOut < windowEnd ? b.CheckOut : windowEnd;
                return Math.Max(0, to.DayNumber - from.DayNumber);
            });
            var pct = capacityNights == 0 ? 0m : Math.Round(booked * 100m / capacityNights, 1);
            return new LabeledValue(h.Name, pct);
        }).ToList();

        return new DashboardStats(
            active.Sum(b => b.TotalPrice), bookings.Count, hotels.Count, hotels.Sum(h => h.Rooms.Count),
            revenueByHotel, byStatus, popular, occupancy);
    }
}

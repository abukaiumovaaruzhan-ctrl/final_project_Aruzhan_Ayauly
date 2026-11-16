using StayEase.Domain;
using StayEase.Infrastructure.Repositories;

namespace StayEase.Services;

public class StatsService(IHotelRepository hotels, IBookingRepository bookings, TimeProvider time)
{
    public async Task<Result<DashboardStats>> GetAsync(CurrentUser actor)
    {
        if (!actor.IsAdmin) return Result<DashboardStats>.Fail(Messages.AdminOnly);
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);
        return Result<DashboardStats>.Ok(StatsCalculator.Compute(await hotels.GetAllAsync(), await bookings.GetAllAsync(), today));
    }
}

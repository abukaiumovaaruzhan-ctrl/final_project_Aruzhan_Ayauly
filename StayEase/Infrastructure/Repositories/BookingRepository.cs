using Microsoft.EntityFrameworkCore;
using StayEase.Domain;
using StayEase.Infrastructure.Data;

namespace StayEase.Infrastructure.Repositories;

public interface IBookingRepository
{
    Task<List<Booking>> GetAllAsync();
    Task<List<Booking>> GetByUserAsync(int userId);
    Task<List<Booking>> GetByRoomAsync(int roomId);
    Task<Booking?> GetAsync(int id);
    Task AddAsync(Booking booking);
    Task UpdateStatusAsync(int id, BookingStatus status);
}

public class BookingRepository(IDbContextFactory<StayEaseDbContext> factory) : IBookingRepository
{
    private static IQueryable<Booking> Full(StayEaseDbContext db) =>
        db.Bookings.Include(b => b.User).Include(b => b.Room).ThenInclude(r => r.Hotel).AsNoTracking();

    public async Task<List<Booking>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await Full(db).OrderByDescending(b => b.Id).ToListAsync();
    }

    public async Task<List<Booking>> GetByUserAsync(int userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await Full(db).Where(b => b.UserId == userId).OrderByDescending(b => b.Id).ToListAsync();
    }

    public async Task<List<Booking>> GetByRoomAsync(int roomId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Bookings.AsNoTracking().Where(b => b.RoomId == roomId).ToListAsync();
    }

    public async Task<Booking?> GetAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await Full(db).FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task AddAsync(Booking booking)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int id, BookingStatus status)
    {
        await using var db = await factory.CreateDbContextAsync();
        var booking = await db.Bookings.FindAsync(id);
        if (booking is null) return;
        booking.Status = status;
        await db.SaveChangesAsync();
    }
}

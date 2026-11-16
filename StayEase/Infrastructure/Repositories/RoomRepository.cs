using Microsoft.EntityFrameworkCore;
using StayEase.Domain;
using StayEase.Infrastructure.Data;

namespace StayEase.Infrastructure.Repositories;

public interface IRoomRepository
{
    Task<Room?> GetAsync(int id);
    Task AddAsync(Room room);
    Task DeleteAsync(int id);
}

public class RoomRepository(IDbContextFactory<StayEaseDbContext> factory) : IRoomRepository
{
    public async Task<Room?> GetAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Rooms.Include(r => r.Hotel).AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(Room room)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var room = await db.Rooms.FindAsync(id);
        if (room is null) return;
        db.Bookings.RemoveRange(db.Bookings.Where(b => b.RoomId == id));
        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using StayEase.Domain;
using StayEase.Infrastructure.Data;

namespace StayEase.Infrastructure.Repositories;

public interface IHotelRepository
{
    Task<List<Hotel>> GetAllAsync();
    Task<Hotel?> GetAsync(int id);
    Task AddAsync(Hotel hotel);
    Task UpdateAsync(Hotel hotel);
    Task DeleteAsync(int id);
}

public class HotelRepository(IDbContextFactory<StayEaseDbContext> factory) : IHotelRepository
{
    public async Task<List<Hotel>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Hotels.Include(h => h.Rooms).AsNoTracking().OrderBy(h => h.Name).ToListAsync();
    }

    public async Task<Hotel?> GetAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Hotels.Include(h => h.Rooms).AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task AddAsync(Hotel hotel)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Hotels.Add(hotel);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Hotel hotel)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Hotels.FindAsync(hotel.Id);
        if (existing is null) return;
        existing.Name = hotel.Name;
        existing.City = hotel.City;
        existing.Address = hotel.Address;
        existing.Description = hotel.Description;
        existing.Stars = hotel.Stars;
        existing.Rating = hotel.Rating;
        existing.ImageUrl = hotel.ImageUrl;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var hotel = await db.Hotels.Include(h => h.Rooms).FirstOrDefaultAsync(h => h.Id == id);
        if (hotel is null) return;
        var roomIds = hotel.Rooms.Select(r => r.Id).ToList();
        // остались только отменённые брони — удаляем их вместе с отелем
        db.Bookings.RemoveRange(db.Bookings.Where(b => roomIds.Contains(b.RoomId)));
        db.Hotels.Remove(hotel);
        await db.SaveChangesAsync();
    }
}

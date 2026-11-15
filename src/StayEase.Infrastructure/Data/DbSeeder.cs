using Microsoft.EntityFrameworkCore;
using StayEase.Domain.Entities;

namespace StayEase.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(StayEaseDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Hotels.AnyAsync()) return;

        var hotel1 = new Hotel { Name = "Grand Astana", Location = "Astana, Kazakhstan", Description = "Comfortable stay in the heart of the city.", Rating = 4.7 };
        var hotel2 = new Hotel { Name = "The Riverside", Location = "Almaty, Kazakhstan", Description = "A calm hotel with mountain views.", Rating = 4.5 };
        hotel1.Rooms.Add(new Room { RoomNumber = "101", Type = "Standard", PricePerNight = 25000, IsAvailable = true });
        hotel1.Rooms.Add(new Room { RoomNumber = "202", Type = "Deluxe", PricePerNight = 42000, IsAvailable = true });
        hotel2.Rooms.Add(new Room { RoomNumber = "12", Type = "Suite", PricePerNight = 55000, IsAvailable = true });
        db.Hotels.AddRange(hotel1, hotel2);
        await db.SaveChangesAsync();
    }
}

using StayEase.Domain.Entities;
using StayEase.Domain.Enums;
using StayEase.Infrastructure.Data; // <-- Добавлено, чтобы видеть базу данных

namespace StayEase.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(StayEaseDbContext context)
        {
            // Если отели уже есть, ничего не делаем
            if (context.Hotels.Any())
            {
                return;
            }

            // Создаем 3 отеля
            var hotel1 = new Hotel
            {
                Name = "Grand Palace Hotel",
                City = "Astana",
                Address = "123 Main St",
                Description = "Enjoy elegant rooms, modern comfort and excellent service.",
                Rating = 4.9m
            };

            var hotel2 = new Hotel
            {
                Name = "Mountain View Resort",
                City = "Almaty",
                Address = "45 Mountain Rd",
                Description = "A relaxing destination surrounded by beautiful mountain views.",
                Rating = 4.8m
            };

            var hotel3 = new Hotel
            {
                Name = "Luxury Garden Hotel",
                City = "Dubai",
                Address = "Palm Jumeirah",
                Description = "Experience luxury, comfort and unforgettable hospitality.",
                Rating = 4.7m
            };

            context.Hotels.AddRange(hotel1, hotel2, hotel3);
            await context.SaveChangesAsync(); // Сохраняем отели, чтобы у них появились ID

            // Добавляем номера для отелей
            var rooms = new List<Room>
            {
                // ВНИМАНИЕ: Если у вас в RoomType.cs другие названия, поменяйте Single и Double на ваши
                new Room { HotelId = hotel1.Id, Name = "Standard Room", Type = RoomType.Single, Capacity = 2, PricePerNight = 120m, IsActive = true },
                new Room { HotelId = hotel1.Id, Name = "Deluxe Suite", Type = RoomType.Double, Capacity = 4, PricePerNight = 250m, IsActive = true },
                new Room { HotelId = hotel2.Id, Name = "Mountain View Room", Type = RoomType.Single, Capacity = 2, PricePerNight = 95m, IsActive = true }
            };

            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();
        }
    }
}
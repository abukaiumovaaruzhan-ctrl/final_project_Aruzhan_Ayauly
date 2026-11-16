using StayEase.Domain;
using StayEase.Services;

namespace StayEase.Infrastructure.Data;

public static class DbInitializer
{
    public const string AdminEmail = "admin@stayease.com";
    public const string AdminPassword = "Admin123!";
    public const string DemoEmail = "demo@stayease.com";
    public const string DemoPassword = "Demo123!";

    /// <summary>Создаёт базу (если её нет) и заполняет её стартовыми данными один раз.</summary>
    public static void Initialize(StayEaseDbContext db, TimeProvider time)
    {
        db.Database.EnsureCreated();
        if (db.Users.Any()) return;

        db.Users.AddRange(
            new User { Name = "Administrator", Email = AdminEmail, PasswordHash = PasswordHasher.Hash(AdminPassword), Role = UserRole.Admin },
            new User { Name = "Demo Customer", Email = DemoEmail, PasswordHash = PasswordHasher.Hash(DemoPassword), Role = UserRole.Customer });

        static string Img(string id) => $"https://images.unsplash.com/{id}?auto=format&fit=crop&w=900&q=70";
        static Room R(string n, RoomType t, decimal p, int c) => new() { Number = n, RoomType = t, Price = p, Capacity = c };

        db.Hotels.AddRange(
            new Hotel { Name = "The Lakeside Grand", City = "Lucerne", Address = "Seestrasse 1", Stars = 5, Rating = 9.4,
                Description = "Belle-époque palace on the water with panoramic Alpine views.", ImageUrl = Img("photo-1566073771259-6a8506099945"),
                Rooms = [R("101", RoomType.Standard, 98000, 2), R("301", RoomType.Suite, 240000, 4)] },
            new Hotel { Name = "Steppe Horizon", City = "Almaty", Address = "Dostyk Ave 120", Stars = 4, Rating = 8.7,
                Description = "Modern rooms at the foot of the Tien Shan mountains.", ImageUrl = Img("photo-1542314831-068cd1dbfeeb"),
                Rooms = [R("12", RoomType.Standard, 35000, 2), R("20", RoomType.Deluxe, 52000, 3)] },
            new Hotel { Name = "Casa Azul", City = "Lisbon", Address = "Rua de São Miguel 14", Stars = 4, Rating = 8.9,
                Description = "Tiled townhouse hotel in the heart of Alfama.", ImageUrl = Img("photo-1551882547-ff40c63fe5fa"),
                Rooms = [R("2A", RoomType.Deluxe, 64000, 2), R("3B", RoomType.Standard, 48000, 2)] },
            new Hotel { Name = "Nordic Pines Lodge", City = "Tromsø", Address = "Fjellveien 3", Stars = 3, Rating = 8.2,
                Description = "Cozy cabins under the northern lights.", ImageUrl = Img("photo-1520250497591-112f2f40a3f4"),
                Rooms = [R("C3", RoomType.Standard, 42000, 2), R("C5", RoomType.Suite, 90000, 5)] },
            new Hotel { Name = "Nomad Palace", City = "Astana", Address = "Kabanbay Batyr Ave 21", Stars = 5, Rating = 9.1,
                Description = "Luxury hotel in the center of the capital with a rooftop restaurant.", ImageUrl = Img("photo-1542314831-068cd1dbfeeb"),
                Rooms = [R("501", RoomType.Deluxe, 75000, 3), R("701", RoomType.Suite, 160000, 4)] });
        db.SaveChanges();

        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);
        var demo = db.Users.First(u => u.Email == DemoEmail);
        var steppeStandard = db.Rooms.First(r => r.Number == "12");
        var lakesideSuite = db.Rooms.First(r => r.Number == "301");

        Booking Make(User u, Room r, int fromDays, int nights, BookingStatus s)
        {
            var ci = today.AddDays(fromDays);
            var co = ci.AddDays(nights);
            return new Booking { UserId = u.Id, RoomId = r.Id, CheckIn = ci, CheckOut = co, Guests = 2,
                TotalPrice = Pricing.CalculatePrice(r.Price, ci, co), Status = s };
        }

        db.Bookings.AddRange(
            Make(demo, steppeStandard, 30, 4, BookingStatus.Confirmed),
            Make(demo, lakesideSuite, 60, 7, BookingStatus.Pending));
        db.SaveChanges();
    }
}

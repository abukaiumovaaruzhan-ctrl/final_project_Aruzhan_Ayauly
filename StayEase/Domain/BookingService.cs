namespace StayEase.Domain;

/// <summary>
/// Хранилище в памяти + бизнес-правила. Списки внутри — неизменяемые:
/// каждая операция создаёт новый список (_data = _data with { ... }).
/// </summary>
public class BookingService
{
    private record Data(
        IReadOnlyList<Hotel> Hotels,
        IReadOnlyList<Room> Rooms,
        IReadOnlyList<Booking> Bookings);

    private readonly object _lock = new();
    private Data _data = Seed();

    private const int CurrentCustomerId = 1;

    private static Data Seed()
    {
        static string Img(string id) =>
            $"https://images.unsplash.com/{id}?auto=format&fit=crop&w=900&q=70";

        return new Data(
            [
                new(1, "The Lakeside Grand", "Lucerne", 5, "Belle-époque palace on the water with panoramic Alpine views.", Img("photo-1566073771259-6a8506099945")),
                new(2, "Steppe Horizon", "Almaty", 4, "Modern rooms at the foot of the Tien Shan mountains.", Img("photo-1542314831-068cd1dbfeeb")),
                new(3, "Casa Azul", "Lisbon", 4, "Tiled townhouse hotel in the heart of Alfama.", Img("photo-1551882547-ff40c63fe5fa")),
                new(4, "Nordic Pines Lodge", "Tromsø", 3, "Cozy cabins under the northern lights.", Img("photo-1520250497591-112f2f40a3f4")),
            ],
            [
                new(1, 1, "101", RoomType.Standard, 220, 2),
                new(2, 1, "301", RoomType.Suite, 540, 4),
                new(3, 2, "12", RoomType.Standard, 90, 2),
                new(4, 2, "20", RoomType.Deluxe, 140, 3),
                new(5, 3, "2A", RoomType.Deluxe, 160, 2),
                new(6, 4, "C3", RoomType.Standard, 110, 2),
            ],
            [
                new(1, 1, 3, new(2026, 12, 20), new(2026, 12, 24), 360, BookingStatus.Confirmed),
                new(2, 2, 2, new(2026, 12, 1), new(2026, 12, 8), 3402, BookingStatus.Pending),
            ]);
    }

    private static int NextId(IEnumerable<int> ids) => ids.DefaultIfEmpty(0).Max() + 1;

    // ---------- Чтение ----------

    public IReadOnlyList<HotelSummary> GetHotels()
    {
        lock (_lock)
        {
            var d = _data;
            return d.Hotels
                .Select(h =>
                {
                    var prices = d.Rooms.Where(r => r.HotelId == h.Id).Select(r => r.Price).ToList();
                    return new HotelSummary(h, prices.Count > 0 ? prices.Min() : null);
                })
                .ToList();
        }
    }

    public HotelDetails? GetHotel(int id)
    {
        lock (_lock)
        {
            var hotel = _data.Hotels.FirstOrDefault(h => h.Id == id);
            return hotel is null
                ? null
                : new HotelDetails(hotel, _data.Rooms.Where(r => r.HotelId == id).ToList());
        }
    }

    public IReadOnlyList<BookingView> GetBookings(int? customerId = null)
    {
        lock (_lock)
        {
            var d = _data;
            return d.Bookings
                .Where(b => customerId is null || b.CustomerId == customerId)
                .Select(b =>
                {
                    var room = d.Rooms.FirstOrDefault(r => r.Id == b.RoomId);
                    var hotel = d.Hotels.FirstOrDefault(h => h.Id == room?.HotelId);
                    return new BookingView(
                        b.Id, b.CustomerId,
                        hotel?.Name ?? "—",
                        room is null ? "—" : $"{room.Type} {room.Number}",
                        b.CheckIn, b.CheckOut, Pricing.Nights(b.CheckIn, b.CheckOut),
                        b.Total, b.Status);
                })
                .ToList();
        }
    }

    public Stats GetStats()
    {
        lock (_lock)
        {
            var revenue = _data.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .Sum(b => b.Total);
            return new Stats(revenue, _data.Bookings.Count, _data.Hotels.Count, _data.Rooms.Count);
        }
    }

    // ---------- Изменение ----------

    public Hotel AddHotel(NewHotel n)
    {
        if (string.IsNullOrWhiteSpace(n.Name) || string.IsNullOrWhiteSpace(n.City))
            throw new DomainException("Name and city are required.");
        if (n.Stars is < 1 or > 5)
            throw new DomainException("Stars must be between 1 and 5.");

        lock (_lock)
        {
            var hotel = new Hotel(
                NextId(_data.Hotels.Select(h => h.Id)),
                n.Name.Trim(), n.City.Trim(), n.Stars,
                n.Description?.Trim() ?? "",
                string.IsNullOrWhiteSpace(n.Image) ? "/hero.jpg" : n.Image);
            _data = _data with { Hotels = [.. _data.Hotels, hotel] };
            return hotel;
        }
    }

    public void DeleteHotel(int id)
    {
        lock (_lock)
        {
            var roomIds = _data.Rooms.Where(r => r.HotelId == id).Select(r => r.Id).ToHashSet();
            if (_data.Bookings.Any(b => roomIds.Contains(b.RoomId) && b.Status != BookingStatus.Cancelled))
                throw new DomainException("This hotel has active bookings.");

            _data = _data with
            {
                Hotels = _data.Hotels.Where(h => h.Id != id).ToList(),
                Rooms = _data.Rooms.Where(r => r.HotelId != id).ToList(),
            };
        }
    }

    public Room AddRoom(NewRoom n)
    {
        if (string.IsNullOrWhiteSpace(n.Number))
            throw new DomainException("Room number is required.");
        if (n.Price <= 0 || n.Capacity <= 0)
            throw new DomainException("Price and capacity must be positive.");

        lock (_lock)
        {
            if (_data.Hotels.All(h => h.Id != n.HotelId))
                throw new DomainException("Hotel not found.");

            var room = new Room(
                NextId(_data.Rooms.Select(r => r.Id)),
                n.HotelId, n.Number.Trim(), n.Type, n.Price, n.Capacity);
            _data = _data with { Rooms = [.. _data.Rooms, room] };
            return room;
        }
    }

    public void DeleteRoom(int id)
    {
        lock (_lock)
        {
            if (_data.Bookings.Any(b => b.RoomId == id && b.Status != BookingStatus.Cancelled))
                throw new DomainException("This room has active bookings.");

            _data = _data with { Rooms = _data.Rooms.Where(r => r.Id != id).ToList() };
        }
    }

    public Booking Book(NewBooking n)
    {
        lock (_lock)
        {
            var room = _data.Rooms.FirstOrDefault(r => r.Id == n.RoomId)
                       ?? throw new DomainException("Room not found.");

            var total = Pricing.CalculatePrice(room.Price, n.CheckIn, n.CheckOut);

            var clash = _data.Bookings.Any(b =>
                b.RoomId == n.RoomId &&
                b.Status != BookingStatus.Cancelled &&
                Pricing.Overlaps(n.CheckIn, n.CheckOut, b.CheckIn, b.CheckOut));
            if (clash)
                throw new DomainException("This room is already booked for those dates.");

            var booking = new Booking(
                NextId(_data.Bookings.Select(b => b.Id)),
                CurrentCustomerId, n.RoomId, n.CheckIn, n.CheckOut, total, BookingStatus.Pending);
            _data = _data with { Bookings = [.. _data.Bookings, booking] };
            return booking;
        }
    }

    public Booking SetStatus(int id, BookingStatus status)
    {
        lock (_lock)
        {
            var booking = _data.Bookings.FirstOrDefault(b => b.Id == id)
                          ?? throw new DomainException("Booking not found.");
            var updated = booking with { Status = status };
            _data = _data with
            {
                Bookings = _data.Bookings.Select(b => b.Id == id ? updated : b).ToList(),
            };
            return updated;
        }
    }
}

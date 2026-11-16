using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StayEase.Infrastructure.Data;
using StayEase.Infrastructure.Repositories;
using StayEase.Services;

namespace StayEase.Tests;

public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

public sealed class SqliteFactory(string connectionString) : IDbContextFactory<StayEaseDbContext>
{
    public StayEaseDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<StayEaseDbContext>().UseSqlite(connectionString).Options);
}

/// <summary>Собирает сервисы поверх настоящей (временной) базы SQLite. Сегодня = 15 ноября 2026.</summary>
public sealed class TestEnv : IDisposable
{
    public static readonly DateOnly Today = new(2026, 11, 15);

    // Идентификаторы из DbInitializer: 1 — администратор, 2 — demo-клиент.
    public static readonly CurrentUser Admin = new(1, "Administrator", true);
    public static readonly CurrentUser Customer = new(2, "Demo Customer", false);

    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"stayease-test-{Guid.NewGuid():N}.db");
    public FixedTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero));

    public BookingService Bookings { get; private set; } = null!;
    public HotelService Hotels { get; private set; } = null!;
    public AuthService Auth { get; private set; } = null!;
    public BookingNotifier Notifier { get; } = new();

    public TestEnv()
    {
        var factory = new SqliteFactory($"Data Source={DbPath}");
        using (var db = factory.CreateDbContext()) DbInitializer.Initialize(db, Time);
        Build(factory);
    }

    /// <summary>Те же сервисы, но «после перезапуска приложения» — тот же файл базы.</summary>
    public TestEnv Restart() => new(DbPath, Time);

    private TestEnv(string existingPath, FixedTimeProvider time)
    {
        DbPath = existingPath;
        Time = time;
        Build(new SqliteFactory($"Data Source={DbPath}"));
        _owner = false;
    }

    private readonly bool _owner = true;

    private void Build(IDbContextFactory<StayEaseDbContext> factory)
    {
        var hotels = new HotelRepository(factory);
        var rooms = new RoomRepository(factory);
        var bookings = new BookingRepository(factory);
        Bookings = new BookingService(bookings, rooms, Time, Notifier);
        Hotels = new HotelService(hotels, rooms, bookings);
        Auth = new AuthService(new UserRepository(factory));
    }

    public async Task<int> FirstRoomIdAsync() => (await Hotels.GetAllAsync()).First(h => h.Name == "The Lakeside Grand").Rooms.First().Id;

    public void Dispose()
    {
        if (!_owner) return;
        SqliteConnection.ClearAllPools();
        try { File.Delete(DbPath); } catch { /* временный файл */ }
    }
}

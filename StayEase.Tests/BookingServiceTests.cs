using StayEase.Domain;

namespace StayEase.Tests;

public class BookingServiceTests
{
    private static readonly DateOnly In = new(2027, 3, 1);
    private static readonly DateOnly Out = new(2027, 3, 5);

    // 1
    [Fact]
    public async Task Successful_booking_is_saved_with_calculated_price()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();

        var result = await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(BookingStatus.Pending, result.Value!.Status);
        Assert.Equal(98000m * 4, result.Value.TotalPrice);
    }

    // 2
    [Fact]
    public async Task Booking_on_taken_dates_fails()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2);

        var second = await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In.AddDays(2), Out.AddDays(2), 2);

        Assert.False(second.IsSuccess);
        Assert.Contains("already booked", second.Error);
    }

    // 3
    [Fact]
    public async Task Booking_with_invalid_dates_fails()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();

        var past = await env.Bookings.CreateAsync(TestEnv.Customer, roomId, TestEnv.Today.AddDays(-1), TestEnv.Today.AddDays(2), 1);
        var reversed = await env.Bookings.CreateAsync(TestEnv.Customer, roomId, Out, In, 1);

        Assert.Contains("past", past.Error);
        Assert.Contains("after check-in", reversed.Error);
    }

    // 4
    [Fact]
    public async Task Booking_over_room_capacity_fails()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync(); // вместимость 2

        var result = await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 3);

        Assert.False(result.IsSuccess);
        Assert.Contains("up to 2 guests", result.Error);
    }

    // 7
    [Fact]
    public async Task Customer_can_cancel_own_booking_and_room_becomes_free()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        var booking = (await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2)).Value!;

        var cancel = await env.Bookings.CancelAsync(TestEnv.Customer, booking.Id);
        var again = await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2);

        Assert.True(cancel.IsSuccess);
        Assert.True(again.IsSuccess);
    }

    // 8
    [Fact]
    public async Task Cancelling_twice_fails()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        var booking = (await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2)).Value!;
        await env.Bookings.CancelAsync(TestEnv.Customer, booking.Id);

        var second = await env.Bookings.CancelAsync(TestEnv.Customer, booking.Id);

        Assert.False(second.IsSuccess);
        Assert.Contains("already cancelled", second.Error);
    }

    [Fact]
    public async Task Customer_cannot_cancel_someone_elses_booking()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        var booking = (await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2)).Value!;

        var other = new CurrentUser(99, "Stranger", false);
        Assert.False((await env.Bookings.CancelAsync(other, booking.Id)).IsSuccess);
    }

    // 11
    [Fact]
    public async Task Admin_can_change_booking_status_and_customer_cannot()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        var booking = (await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2)).Value!;

        Assert.False((await env.Bookings.SetStatusAsync(TestEnv.Customer, booking.Id, BookingStatus.Confirmed)).IsSuccess);
        Assert.True((await env.Bookings.SetStatusAsync(TestEnv.Admin, booking.Id, BookingStatus.Confirmed)).IsSuccess);

        var mine = await env.Bookings.GetMineAsync(TestEnv.Customer);
        Assert.Equal(BookingStatus.Confirmed, mine.First(b => b.Id == booking.Id).Status);
    }

    // 15
    [Fact]
    public async Task User_sees_only_own_bookings()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        await env.Bookings.CreateAsync(TestEnv.Admin, roomId, In, Out, 1);

        var demo = await env.Bookings.GetMineAsync(TestEnv.Customer);
        var admin = await env.Bookings.GetMineAsync(TestEnv.Admin);

        Assert.All(demo, b => Assert.Equal(TestEnv.Customer.Id, b.UserId));
        Assert.Single(admin);
    }

    [Fact]
    public async Task Non_admin_cannot_list_all_bookings()
    {
        using var env = new TestEnv();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => env.Bookings.GetAllAsync(TestEnv.Customer));
    }

    [Fact]
    public async Task Creating_a_booking_notifies_subscribers()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        BookingEvent? received = null;
        env.Notifier.OnEvent += e => received = e;

        await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2);

        Assert.NotNull(received);
        Assert.Equal(BookingEventKind.Created, received!.Kind);
    }

    [Fact]
    public async Task Data_survives_application_restart()
    {
        using var env = new TestEnv();
        var roomId = await env.FirstRoomIdAsync();
        var created = (await env.Bookings.CreateAsync(TestEnv.Customer, roomId, In, Out, 2)).Value!;

        var restarted = env.Restart(); // новый набор сервисов поверх того же файла базы
        var mine = await restarted.Bookings.GetMineAsync(TestEnv.Customer);

        Assert.Contains(mine, b => b.Id == created.Id);
        Assert.NotEmpty(await restarted.Hotels.GetAllAsync());
    }
}

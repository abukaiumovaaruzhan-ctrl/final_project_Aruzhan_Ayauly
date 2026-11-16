using StayEase.Domain;
using StayEase.Infrastructure.Repositories;

namespace StayEase.Services;

public class BookingService(IBookingRepository bookings, IRoomRepository rooms, TimeProvider time, BookingNotifier notifier)
{
    public DateOnly Today => DateOnly.FromDateTime(time.GetLocalNow().DateTime);

    public async Task<Result<Booking>> CreateAsync(CurrentUser user, int roomId, DateOnly checkIn, DateOnly checkOut, int guests)
    {
        var room = await rooms.GetAsync(roomId);
        if (room is null) return Result<Booking>.Fail("Room not found.");

        var existing = await bookings.GetByRoomAsync(roomId);
        var error = BookingRules.Validate(checkIn, checkOut, guests, room, Today, existing);
        if (error is not null) return Result<Booking>.Fail(error);

        var price = Pricing.Breakdown(room.Price, checkIn, checkOut);
        var booking = new Booking
        {
            UserId = user.Id, RoomId = roomId, CheckIn = checkIn, CheckOut = checkOut,
            Guests = guests, TotalPrice = price.Total, Status = BookingStatus.Pending,
        };
        await bookings.AddAsync(booking);

        notifier.Publish(new BookingEvent(BookingEventKind.Created, booking.Id, user.Id, user.Id,
            $"New booking #{booking.Id} for {room.Hotel.Name}."));

        return Result<Booking>.Ok(await bookings.GetAsync(booking.Id) ?? booking);
    }

    public async Task<Result> CancelAsync(CurrentUser actor, int bookingId)
    {
        var booking = await bookings.GetAsync(bookingId);
        if (booking is null) return Result.Fail("Booking not found.");
        if (booking.UserId != actor.Id && !actor.IsAdmin) return Result.Fail("You can only cancel your own bookings.");
        if (booking.Status == BookingStatus.Cancelled) return Result.Fail("This booking is already cancelled.");

        await bookings.UpdateStatusAsync(bookingId, BookingStatus.Cancelled);
        notifier.Publish(new BookingEvent(BookingEventKind.Cancelled, bookingId, booking.UserId, actor.Id,
            $"Booking #{bookingId} was cancelled."));
        return Result.Ok();
    }

    public async Task<Result> SetStatusAsync(CurrentUser actor, int bookingId, BookingStatus status)
    {
        if (!actor.IsAdmin) return Result.Fail(Messages.AdminOnly);
        var booking = await bookings.GetAsync(bookingId);
        if (booking is null) return Result.Fail("Booking not found.");
        if (booking.Status == status) return Result.Fail($"Booking is already {status}.");
        if (booking.Status == BookingStatus.Cancelled) return Result.Fail("A cancelled booking cannot be changed.");

        await bookings.UpdateStatusAsync(bookingId, status);
        notifier.Publish(new BookingEvent(BookingEventKind.StatusChanged, bookingId, booking.UserId, actor.Id,
            $"Booking #{bookingId} is now {status}."));
        return Result.Ok();
    }

    public Task<List<Booking>> GetMineAsync(CurrentUser user) => bookings.GetByUserAsync(user.Id);

    public Task<List<Booking>> GetAllAsync(CurrentUser actor) =>
        actor.IsAdmin ? bookings.GetAllAsync() : throw new UnauthorizedAccessException(Messages.AdminOnly);

    /// <summary>Занятые периоды комнаты (для календаря доступности).</summary>
    public async Task<List<(DateOnly Start, DateOnly End)>> GetOccupiedAsync(int roomId) =>
        (await bookings.GetByRoomAsync(roomId))
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Select(b => (b.CheckIn, b.CheckOut))
            .ToList();
}

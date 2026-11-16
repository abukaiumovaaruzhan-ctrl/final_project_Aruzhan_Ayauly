using StayEase.Domain;
using StayEase.Infrastructure.Repositories;

namespace StayEase.Services;

public class HotelService(IHotelRepository hotels, IRoomRepository rooms, IBookingRepository bookings)
{
    public Task<List<Hotel>> GetAllAsync() => hotels.GetAllAsync();
    public Task<Hotel?> GetAsync(int id) => hotels.GetAsync(id);

    private static Hotel Build(int id, HotelInput i) => new()
    {
        Id = id, Name = i.Name.Trim(), City = i.City.Trim(), Address = (i.Address ?? "").Trim(),
        Description = (i.Description ?? "").Trim(), Stars = i.Stars, Rating = i.Rating,
        ImageUrl = string.IsNullOrWhiteSpace(i.ImageUrl) ? "/hero.jpg" : i.ImageUrl.Trim(),
    };

    public async Task<Result<Hotel>> AddAsync(CurrentUser actor, HotelInput input)
    {
        if (!actor.IsAdmin) return Result<Hotel>.Fail(Messages.AdminOnly);
        var error = CatalogRules.Validate(input);
        if (error is not null) return Result<Hotel>.Fail(error);

        var hotel = Build(0, input);
        await hotels.AddAsync(hotel);
        return Result<Hotel>.Ok(hotel);
    }

    public async Task<Result<Hotel>> UpdateAsync(CurrentUser actor, int id, HotelInput input)
    {
        if (!actor.IsAdmin) return Result<Hotel>.Fail(Messages.AdminOnly);
        if (await hotels.GetAsync(id) is null) return Result<Hotel>.Fail("Hotel not found.");
        var error = CatalogRules.Validate(input);
        if (error is not null) return Result<Hotel>.Fail(error);

        var hotel = Build(id, input);
        await hotels.UpdateAsync(hotel);
        return Result<Hotel>.Ok(hotel);
    }

    public async Task<Result> DeleteAsync(CurrentUser actor, int id)
    {
        if (!actor.IsAdmin) return Result.Fail(Messages.AdminOnly);
        var hotel = await hotels.GetAsync(id);
        if (hotel is null) return Result.Fail("Hotel not found.");

        var roomIds = hotel.Rooms.Select(r => r.Id).ToHashSet();
        var hasActive = (await bookings.GetAllAsync())
            .Any(b => roomIds.Contains(b.RoomId) && b.Status != BookingStatus.Cancelled);
        if (hasActive) return Result.Fail("This hotel has active bookings.");

        await hotels.DeleteAsync(id);
        return Result.Ok();
    }

    public async Task<Result<Room>> AddRoomAsync(CurrentUser actor, RoomInput input)
    {
        if (!actor.IsAdmin) return Result<Room>.Fail(Messages.AdminOnly);
        var error = CatalogRules.Validate(input);
        if (error is not null) return Result<Room>.Fail(error);
        if (await hotels.GetAsync(input.HotelId) is null) return Result<Room>.Fail("Hotel not found.");

        var room = new Room
        {
            HotelId = input.HotelId, Number = input.Number.Trim(), RoomType = input.RoomType,
            Price = input.Price, Capacity = input.Capacity,
        };
        await rooms.AddAsync(room);
        return Result<Room>.Ok(room);
    }

    public async Task<Result> DeleteRoomAsync(CurrentUser actor, int roomId)
    {
        if (!actor.IsAdmin) return Result.Fail(Messages.AdminOnly);
        if (await rooms.GetAsync(roomId) is null) return Result.Fail("Room not found.");

        var hasActive = (await bookings.GetByRoomAsync(roomId)).Any(b => b.Status != BookingStatus.Cancelled);
        if (hasActive) return Result.Fail("This room has active bookings.");

        await rooms.DeleteAsync(roomId);
        return Result.Ok();
    }
}

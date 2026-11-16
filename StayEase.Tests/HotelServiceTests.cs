using StayEase.Domain;

namespace StayEase.Tests;

public class HotelServiceTests
{
    private static readonly HotelInput NewHotel = new("Test Inn", "Astana", "Main St 1", "Nice", 4, 8.5, null);

    // 9
    [Fact]
    public async Task Admin_can_create_hotel()
    {
        using var env = new TestEnv();

        var result = await env.Hotels.AddAsync(TestEnv.Admin, NewHotel);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Contains(await env.Hotels.GetAllAsync(), h => h.Name == "Test Inn");
    }

    [Fact]
    public async Task Customer_cannot_create_hotel()
    {
        using var env = new TestEnv();
        var result = await env.Hotels.AddAsync(TestEnv.Customer, NewHotel);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Hotel_validation_rejects_bad_input()
    {
        using var env = new TestEnv();
        Assert.False((await env.Hotels.AddAsync(TestEnv.Admin, NewHotel with { Name = " " })).IsSuccess);
        Assert.False((await env.Hotels.AddAsync(TestEnv.Admin, NewHotel with { Stars = 9 })).IsSuccess);
    }

    [Fact]
    public async Task Admin_can_edit_hotel()
    {
        using var env = new TestEnv();
        var hotel = (await env.Hotels.AddAsync(TestEnv.Admin, NewHotel)).Value!;

        await env.Hotels.UpdateAsync(TestEnv.Admin, hotel.Id, NewHotel with { Name = "Renamed Inn" });

        Assert.Equal("Renamed Inn", (await env.Hotels.GetAsync(hotel.Id))!.Name);
    }

    // 10
    [Fact]
    public async Task Hotel_without_active_bookings_can_be_deleted_but_with_them_cannot()
    {
        using var env = new TestEnv();
        var fresh = (await env.Hotels.AddAsync(TestEnv.Admin, NewHotel)).Value!;
        Assert.True((await env.Hotels.DeleteAsync(TestEnv.Admin, fresh.Id)).IsSuccess);
        Assert.Null(await env.Hotels.GetAsync(fresh.Id));

        var steppe = (await env.Hotels.GetAllAsync()).First(h => h.Name == "Steppe Horizon"); // у него есть активная бронь
        var blocked = await env.Hotels.DeleteAsync(TestEnv.Admin, steppe.Id);
        Assert.False(blocked.IsSuccess);
        Assert.Contains("active bookings", blocked.Error);
    }

    [Fact]
    public async Task Admin_can_add_and_delete_room()
    {
        using var env = new TestEnv();
        var hotel = (await env.Hotels.AddAsync(TestEnv.Admin, NewHotel)).Value!;

        var room = await env.Hotels.AddRoomAsync(TestEnv.Admin, new RoomInput(hotel.Id, "A1", RoomType.Deluxe, 50000, 2));
        Assert.True(room.IsSuccess, room.Error);
        Assert.True((await env.Hotels.DeleteRoomAsync(TestEnv.Admin, room.Value!.Id)).IsSuccess);
    }
}

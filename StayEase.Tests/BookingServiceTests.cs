using Xunit;
using StayEase.Domain;

namespace StayEase.Tests;

public class BookingServiceTests
{
    [Fact]
    public void Cannot_book_same_room_twice_for_overlapping_dates()
    {
        var s = new BookingService();
        s.Book(new NewBooking(1, new(2027, 3, 1), new(2027, 3, 5)));

        Assert.Throws<DomainException>(() =>
            s.Book(new NewBooking(1, new(2027, 3, 4), new(2027, 3, 6))));
    }

    [Fact]
    public void Cancelled_booking_frees_the_room()
    {
        var s = new BookingService();
        var b = s.Book(new NewBooking(1, new(2027, 3, 1), new(2027, 3, 5)));
        s.SetStatus(b.Id, BookingStatus.Cancelled);

        var again = s.Book(new NewBooking(1, new(2027, 3, 1), new(2027, 3, 5)));
        Assert.Equal(BookingStatus.Pending, again.Status);
    }

    [Fact]
    public void Cannot_delete_room_with_active_booking()
    {
        var s = new BookingService();
        Assert.Throws<DomainException>(() => s.DeleteRoom(3)); // у комнаты 3 есть активная бронь
    }
}

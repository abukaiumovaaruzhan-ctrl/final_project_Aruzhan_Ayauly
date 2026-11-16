using StayEase.Domain;

namespace StayEase.Services;

/// <summary>
/// Состояние бронирования, общее для компонентов (карточка отеля, календарь, форма, итог).
/// Любое изменение вызывает событие OnChange, и подписанные компоненты перерисовываются.
/// </summary>
public class BookingState
{
    public Hotel? Hotel { get; private set; }
    public Room? Room { get; private set; }
    public DateOnly? CheckIn { get; private set; }
    public DateOnly? CheckOut { get; private set; }
    public int Guests { get; private set; } = 1;
    public int BookingsVersion { get; private set; }

    public event Action? OnChange;

    public PriceBreakdown? Breakdown =>
        Room is not null && CheckIn is { } ci && CheckOut is { } co && co > ci
            ? Pricing.Breakdown(Room.Price, ci, co)
            : null;

    public void SelectHotel(Hotel hotel)
    {
        if (Hotel?.Id != hotel.Id)
        {
            Room = null; CheckIn = null; CheckOut = null; Guests = 1;
        }
        Hotel = hotel;
        Notify();
    }

    public void SelectRoom(Room room)
    {
        Room = room;
        if (Guests > room.Capacity) Guests = room.Capacity;
        Notify();
    }

    public void SetDates(DateOnly? checkIn, DateOnly? checkOut)
    {
        CheckIn = checkIn;
        CheckOut = checkOut;
        Notify();
    }

    public void SetGuests(int guests)
    {
        Guests = Math.Max(1, guests);
        Notify();
    }

    /// <summary>После успешного бронирования: сбросить даты и обновить календарь.</summary>
    public void MarkBooked()
    {
        CheckIn = null; CheckOut = null;
        BookingsVersion++;
        Notify();
    }

    private void Notify() => OnChange?.Invoke();
}

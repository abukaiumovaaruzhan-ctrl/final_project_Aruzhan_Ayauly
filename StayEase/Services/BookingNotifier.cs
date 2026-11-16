namespace StayEase.Services;

public enum BookingEventKind { Created, StatusChanged, Cancelled }

/// <param name="OwnerUserId">Чьё это бронирование.</param>
/// <param name="ActorId">Кто совершил действие.</param>
public record BookingEvent(BookingEventKind Kind, int BookingId, int OwnerUserId, int ActorId, string Message);

/// <summary>
/// Шина уведомлений. Singleton: события доходят до всех открытых Blazor-сессий
/// (каждая сессия — это SignalR-подключение).
/// </summary>
public class BookingNotifier
{
    public event Action<BookingEvent>? OnEvent;

    public void Publish(BookingEvent e)
    {
        var handlers = OnEvent;
        if (handlers is null) return;
        foreach (var d in handlers.GetInvocationList())
        {
            try { ((Action<BookingEvent>)d)(e); } catch { /* закрытая вкладка не должна ломать остальных */ }
        }
    }
}

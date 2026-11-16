namespace StayEase.Services;

public enum ToastKind { Success, Error, Info }

public record ToastMessage(Guid Id, string Text, ToastKind Kind);

public class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Show(string text, ToastKind kind) => OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), text, kind));
    public void Success(string text) => Show(text, ToastKind.Success);
    public void Error(string text) => Show(text, ToastKind.Error);
    public void Info(string text) => Show(text, ToastKind.Info);
}

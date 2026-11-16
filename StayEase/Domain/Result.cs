namespace StayEase.Domain;

/// <summary>Результат операции без значения: либо успех, либо текст ошибки.</summary>
public record Result(string? Error)
{
    public bool IsSuccess => Error is null;
    public static Result Ok() => new((string?)null);
    public static Result Fail(string error) => new(error);
}

/// <summary>Результат операции со значением (вместо исключений для бизнес-ошибок).</summary>
public record Result<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;
    public static Result<T> Ok(T value) => new(value, null);
    public static Result<T> Fail(string error) => new(default, error);
    public Result ToResult() => new(Error);
}

public static class Messages
{
    public const string AdminOnly = "Admin access required.";
}

namespace PruebaNetCoreProject.Application.Common;

public sealed class Result<TValue>
{
    private readonly TValue? _value;

    private Result(bool isSuccess, TValue? value, string? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(true, value, null);
    }

    public static Result<TValue> Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new Result<TValue>(false, default, error);
    }
}

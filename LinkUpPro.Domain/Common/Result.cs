namespace LinkUpPro.Domain.Common;

/// <summary>
/// Represents the result of a domain operation that does not return a value.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, IReadOnlyCollection<DomainError> errors)
    {
        if (isSuccess && errors.Count > 0)
        {
            throw new InvalidOperationException("A successful result cannot contain errors.");
        }

        if (!isSuccess && errors.Count == 0)
        {
            throw new InvalidOperationException("A failed result must contain at least one error.");
        }

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<DomainError> Errors { get; }

    public DomainError Error => Errors.FirstOrDefault() ?? DomainError.None;

    public static Result Success() => new(true, []);

    public static Result Failure(DomainError error) => new(false, [error]);

    public static Result Failure(IEnumerable<DomainError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.Where(error => error != DomainError.None).ToArray();

        return new Result(false, errorList);
    }
}

/// <summary>
/// Represents the result of a domain operation that returns a value.
/// </summary>
/// <typeparam name="T">Returned value type.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(true, [])
    {
        _value = value;
    }

    private Result(IReadOnlyCollection<DomainError> errors)
        : base(false, errors)
    {
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(DomainError error) => new([error]);

    public static new Result<T> Failure(IEnumerable<DomainError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.Where(error => error != DomainError.None).ToArray();

        return new Result<T>(errorList);
    }
}

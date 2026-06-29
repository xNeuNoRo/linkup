namespace LinkUpPro.Domain.Common;

/// <summary>
/// Representa el resultado de una operación de dominio que no devuelve un valor.
/// </summary>
public class Result
{
    // El constructor es protegido para forzar el uso de los métodos factory estáticos para crear instancias de Result.
    protected Result(bool isSuccess, IReadOnlyCollection<DomainError> errors)
    {
        if (isSuccess && errors.Count > 0)
        {
            throw new InvalidOperationException("Una operación exitosa no debe contener errores.");
        }

        if (!isSuccess && errors.Count == 0)
        {
            throw new InvalidOperationException(
                "Una operación fallida debe contener al menos un error."
            );
        }

        IsSuccess = isSuccess;
        Errors = errors;
    }

    // Propiedades para indicar si la operación fue exitosa o fallida, y para contener los errores en caso de fallo.
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
/// Representa el resultado de una operación de dominio que devuelve un valor de tipo T.
/// </summary>
/// <typeparam name="T">El tipo del valor devuelto.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    // Constructor de exito con valor
    private Result(T value)
        : base(true, [])
    {
        _value = value;
    }

    // Constructor de fallo con errores
    private Result(IReadOnlyCollection<DomainError> errors)
        : base(false, errors) { }

    // El valor solo puede ser accedido si el resultado es exitoso
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "El valor de un resultado fallido no puede ser accedido."
            );

    // Funciones factory para crear resultados exitosos o fallidos
    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(DomainError error) => new([error]);

    public static new Result<T> Failure(IEnumerable<DomainError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.Where(error => error != DomainError.None).ToArray();

        return new Result<T>(errorList);
    }
}

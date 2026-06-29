using System.Text.RegularExpressions;
using LinkUpPro.Domain.Exceptions;

namespace LinkUpPro.Domain.ValueObjects;

/// <summary>
/// Representa un número de teléfono con validación integrada para asegurar que el formato
/// sea válido para números de República Dominicana, con el formato "809-XXX-XXXX",
/// "829-XXX-XXXX" o "849-XXX-XXXX".
/// </summary>
public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^(809|829|849)-\d{3}-\d{4}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public string DigitsOnly => Value.Replace("-", string.Empty, StringComparison.Ordinal);

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(
                "Debe ingresar un numero telefonico valido de Republica Dominicana.",
                "User.PhoneNumberRequired"
            );
        }

        var normalized = value.Trim();

        if (!PhoneRegex.IsMatch(normalized))
        {
            throw new DomainException(
                "Debe ingresar un numero telefonico valido de Republica Dominicana.",
                "User.InvalidPhoneNumber"
            );
        }

        return new PhoneNumber(normalized);
    }

    public override string ToString() => Value;
}

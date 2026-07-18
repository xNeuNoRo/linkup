using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que un rango de fechas tenga FechaDesde <= FechaHasta.
/// Se aplica a la propiedad de inicio (From).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DateRangeAttribute : ValidationAttribute
{
    public string OtherProperty { get; }

    public DateRangeAttribute(string otherProperty)
        : base("La fecha inicial no puede ser posterior a la fecha final.")
    {
        OtherProperty = otherProperty;
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    )
    {
        if (value is null) return ValidationResult.Success;

        var otherProp = validationContext.ObjectType.GetProperty(OtherProperty);
        if (otherProp is null) return ValidationResult.Success;

        var otherValue = otherProp.GetValue(validationContext.ObjectInstance);
        if (otherValue is null) return ValidationResult.Success;

        DateTime from = value switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            _ => DateTime.MinValue
        };

        DateTime to = otherValue switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            _ => DateTime.MinValue
        };

        if (from > to)
        {
            return new ValidationResult(
                ErrorMessage,
                new[] { validationContext.MemberName!, OtherProperty }
            );
        }

        return ValidationResult.Success;
    }
}

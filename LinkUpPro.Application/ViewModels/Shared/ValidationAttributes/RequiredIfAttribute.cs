using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que la propiedad actual sea requerida cuando otra propiedad tenga un valor específico.
/// Útil para validaciones condicionales como:
/// - CreatePost: ImageFile requerido si ContentType == 1
/// - ChangePassword: CurrentPassword requerido si NewPassword tiene valor
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class RequiredIfAttribute : ValidationAttribute
{
    public string DependentProperty { get; }
    public object? TargetValue { get; }
    public bool IsInverted { get; }

    public RequiredIfAttribute(string dependentProperty, object targetValue, bool inverted = false)
        : base("El campo {0} es requerido cuando {1} {2}.")
    {
        DependentProperty = dependentProperty;
        TargetValue = targetValue;
        IsInverted = inverted;
    }

    public override string FormatErrorMessage(string name)
    {
        var condition = IsInverted ? "no es" : "es";
        return string.Format(ErrorMessageString, name, DependentProperty, $"{condition} {TargetValue}");
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    )
    {
        var dependentProp = validationContext.ObjectType.GetProperty(
            DependentProperty,
            BindingFlags.Public | BindingFlags.Instance
        );

        if (dependentProp is null)
            return new ValidationResult($"Propiedad dependiente '{DependentProperty}' no encontrada.");

        var dependentValue = dependentProp.GetValue(validationContext.ObjectInstance);

        bool conditionMet = IsInverted
            ? !Equals(dependentValue, TargetValue)
            : Equals(dependentValue, TargetValue);

        if (conditionMet)
        {
            bool hasValue = value switch
            {
                null => false,
                string s => !string.IsNullOrWhiteSpace(s),
                IFormFile file => file.Length > 0,
                _ => true
            };

            if (!hasValue)
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.MemberName ?? validationContext.DisplayName),
                    new[] { validationContext.MemberName! }
                );
            }
        }

        return ValidationResult.Success;
    }
}

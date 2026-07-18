using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que dos propiedades de un ViewModel no tengan ambas un valor al mismo tiempo.
/// Útil para CreatePostViewModel: ImageFile y YouTubeUrl son mutuamente excluyentes.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MutuallyExclusiveWithAttribute : ValidationAttribute
{
    public string OtherProperty { get; }

    public MutuallyExclusiveWithAttribute(string otherProperty)
        : base("No debe completar ambos campos al mismo tiempo.")
    {
        OtherProperty = otherProperty;
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    )
    {
        var currentProp = validationContext.ObjectType.GetProperty(validationContext.MemberName!);
        if (currentProp is null) return ValidationResult.Success;

        var otherProp = validationContext.ObjectType.GetProperty(OtherProperty);
        if (otherProp is null) return ValidationResult.Success;

        var currentValue = currentProp.GetValue(validationContext.ObjectInstance);
        var otherValue = otherProp.GetValue(validationContext.ObjectInstance);

        bool currentHasValue = HasValue(currentValue);
        bool otherHasValue = HasValue(otherValue);

        if (currentHasValue && otherHasValue)
        {
            return new ValidationResult(
                ErrorMessage,
                new[] { validationContext.MemberName!, OtherProperty }
            );
        }

        return ValidationResult.Success;
    }

    private static bool HasValue(object? value)
    {
        if (value is null) return false;
        if (value is string s) return !string.IsNullOrWhiteSpace(s);
        if (value is Microsoft.AspNetCore.Http.IFormFile f) return f.Length > 0;
        return true;
    }
}

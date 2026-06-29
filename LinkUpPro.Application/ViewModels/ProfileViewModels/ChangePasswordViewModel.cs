using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

namespace LinkUpPro.Application.ViewModels.ProfileViewModels;

/// <summary>
/// ViewModel para la sección de cambio de contraseña dentro de "Mi Perfil".
/// Los 3 campos son opcionales. Si se llena cualquiera de los 3, se requieren los 3
/// (validación IValidatableObject + atributos RequiredIf para client-side).
/// </summary>
public class ChangePasswordViewModel : IValidatableObject
{
    [DataType(DataType.Password)]
    [RequiredIf(nameof(NewPassword), "", inverted: true, ErrorMessage = "Para cambiar su contraseña debe completar la contraseña actual, la nueva contraseña y su confirmación.")]
    [Display(Name = "Contraseña actual")]
    public string CurrentPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial."
    )]
    [Display(Name = "Nueva contraseña")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [RequiredIf(nameof(NewPassword), "", inverted: true, ErrorMessage = "Debe confirmar la nueva contraseña.")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var anyField = !string.IsNullOrWhiteSpace(CurrentPassword) ||
                       !string.IsNullOrWhiteSpace(NewPassword) ||
                       !string.IsNullOrWhiteSpace(ConfirmPassword);

        if (anyField)
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword))
                yield return new ValidationResult("La contraseña actual es requerida.", new[] { nameof(CurrentPassword) });

            if (string.IsNullOrWhiteSpace(NewPassword))
                yield return new ValidationResult("La nueva contraseña es requerida.", new[] { nameof(NewPassword) });

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
                yield return new ValidationResult("La confirmación de contraseña es requerida.", new[] { nameof(ConfirmPassword) });

            if (!string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                if (NewPassword != ConfirmPassword)
                    yield return new ValidationResult("La nueva contraseña y su confirmación no coinciden.", new[] { nameof(ConfirmPassword) });

                if (!string.IsNullOrWhiteSpace(CurrentPassword) && NewPassword == CurrentPassword)
                    yield return new ValidationResult("La nueva contraseña debe ser diferente de la contraseña actual.", new[] { nameof(NewPassword) });
            }
        }
    }
}

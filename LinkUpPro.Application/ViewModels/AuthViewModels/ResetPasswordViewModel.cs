using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.AuthViewModels;

/// <summary>
/// ViewModel para la pantalla "Registrar nueva contraseña" (al click en enlace del email).
/// Contiene el token y el userId (recibidos por query string, campos ocultos)
/// y los campos de nueva contraseña y confirmación.
/// </summary>
public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "El identificador de usuario es requerido.")]
    [Display(Name = "Identificador de usuario")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El token es requerido.")]
    [Display(Name = "Token de restablecimiento")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial."
    )]
    [Display(Name = "Nueva contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es requerida.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.AuthViewModels;

/// <summary>
/// ViewModel para la pantalla de inicio de sesión.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener sesión iniciada")]
    public bool RememberMe { get; set; }
}

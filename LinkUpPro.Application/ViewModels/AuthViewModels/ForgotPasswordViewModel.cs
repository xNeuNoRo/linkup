using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.AuthViewModels;

/// <summary>
/// ViewModel para la pantalla "Restablecer contraseña" (solicitud de enlace).
/// Solo requiere el nombre de usuario. La respuesta siempre es genérica
/// (no revela si la cuenta existe) — eso se valida en el servidor.
/// </summary>
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.AuthViewModels;

/// <summary>
/// ViewModel para la pantalla "Reenviar correo de activación".
/// Similar a ForgotPassword: solo pide el nombre de usuario.
/// La respuesta siempre es genérica (no revela existencia de la cuenta).
/// </summary>
public class ResendActivationViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;
}

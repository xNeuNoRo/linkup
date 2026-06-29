namespace LinkUpPro.Application.ViewModels.AuthViewModels;

/// <summary>
/// ViewModel informativo (sin formulario) para la pantalla de activación de cuenta.
/// Se usa cuando el usuario hace click en el enlace del correo de activación.
/// Los parámetros (userId, token) vienen por query string.
/// </summary>
public class ActivateAccountViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la activación fue exitosa.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Mensaje a mostrar al usuario.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

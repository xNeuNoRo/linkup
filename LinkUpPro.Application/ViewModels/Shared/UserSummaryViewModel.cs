namespace LinkUpPro.Application.ViewModels.Shared;

/// <summary>
/// Resumen básico de un usuario (para listas, navbar, avatares, etc.).
/// </summary>
public class UserSummaryViewModel
{
    /// <summary>
    /// Identificador del usuario.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario (username).
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo (Nombre + Apellido).
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Ruta de la foto de perfil.
    /// </summary>
    public string? ProfilePicturePath { get; set; }

    /// <summary>
    /// Indica si la cuenta está activa.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

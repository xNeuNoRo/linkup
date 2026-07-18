using System.Security.Claims;

namespace LinkUpPro.Application.Interfaces.Services;

/// <summary>
/// Proporciona acceso a la identidad e información del usuario autenticado en la sesión actual.
/// Desacopla la lógica de negocio de la infraestructura de ASP.NET Core Identity.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID del usuario autenticado (string porque IdentityUser usa string).
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Nombre de usuario (UserName de Identity).
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Nombre completo del usuario (FirstName + LastName).
    /// </summary>
    string? FullName { get; }

    /// <summary>
    /// URL/ruta de la foto de perfil del usuario.
    /// </summary>
    string? ProfilePicturePath { get; }

    /// <summary>
    /// Indica si hay una sesión activa (usuario autenticado).
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Indica si el usuario seleccionó "Recordar sesión" al iniciar sesión.
    /// </summary>
    bool RememberMe { get; }

    /// <summary>
    /// Última fecha/hora de actividad del usuario (UTC).
    /// </summary>
    DateTimeOffset? LastActivityAt { get; }

    /// <summary>
    /// Indica si la cuenta del usuario está activa.
    /// Requiere una llamada a BD, por lo que es async.
    /// </summary>
    Task<bool> IsActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el ClaimsPrincipal del usuario actual.
    /// </summary>
    ClaimsPrincipal? GetPrincipal();

    /// <summary>
    /// Actualiza la marca de tiempo de última actividad del usuario.
    /// Debe llamarse en cada request autenticado para mantener la sesión viva.
    /// </summary>
    Task TouchLastActivityAsync(CancellationToken cancellationToken = default);
}

namespace LinkUpPro.Application.DTOs.Profile.Responses;

/// <summary>
/// Respuesta de operaciones de edicion de perfil o cambio de contrasena.
/// </summary>
public record EditProfileResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    bool IsVerified,
    bool HasError,
    List<string> Errors,
    bool RequiresReLogin
);

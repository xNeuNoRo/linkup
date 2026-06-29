namespace LinkUpPro.Application.DTOs.Profile.Responses;

/// <summary>
/// Perfil completo del usuario para la vista "Mi Perfil".
/// </summary>
public record UserProfileResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Email,
    string UserName,
    string? ProfilePicturePath,
    bool IsActive,
    bool IsVerified,
    DateTime CreatedAt,
    DateTime? LastActivityAt
);

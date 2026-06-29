namespace LinkUpPro.Application.DTOs.Profile.Responses;

/// <summary>
/// Informacion basica del usuario para busquedas y lookups.
/// </summary>
public record UserResponseDto(
    string Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfilePicturePath,
    bool IsActive,
    bool IsVerified
);

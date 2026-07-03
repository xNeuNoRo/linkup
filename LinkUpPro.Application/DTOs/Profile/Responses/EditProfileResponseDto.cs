namespace LinkUpPro.Application.DTOs.Profile.Responses;

public record EditProfileResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    string? ProfilePicturePath,
    bool IsVerified,
    bool RequiresReLogin
);

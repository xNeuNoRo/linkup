namespace LinkUpPro.Application.DTOs.Profile.Responses;

public record EditProfileResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    bool IsVerified,
    bool RequiresReLogin
);

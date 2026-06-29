namespace LinkUpPro.Application.DTOs.User.Responses;

public record UserDto(
    string Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfilePicturePath,
    bool IsActive,
    bool EmailConfirmed
);

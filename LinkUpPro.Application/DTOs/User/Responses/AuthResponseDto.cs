namespace LinkUpPro.Application.DTOs.User.Responses;

public record AuthResponseDto(
    string Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? ProfilePicturePath,
    bool IsVerified,
    List<string>? Roles = null
);

namespace LinkUpPro.Application.DTOs.User.Responses;

public record AuthResponseDto
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfilePicturePath { get; init; }
    public bool IsVerified { get; init; }
    public bool HasError { get; init; }
    public List<string> Errors { get; init; } = [];
    public List<string>? Roles { get; init; }

    public static AuthResponseDto CreateSuccess(
        string id,
        string userName,
        string email,
        string firstName,
        string lastName,
        string? profilePicturePath,
        bool isVerified,
        List<string>? roles = null
    ) =>
        new()
        {
            Id = id,
            UserName = userName,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ProfilePicturePath = profilePicturePath,
            IsVerified = isVerified,
            Roles = roles,
        };

    public static AuthResponseDto CreateError(List<string> errors) =>
        new() { HasError = true, Errors = errors };
}

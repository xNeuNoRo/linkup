namespace LinkUpPro.Application.DTOs.User.Responses;

public record UserProfileResponseDto
{
    public string Id { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string? ProfilePicturePath { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastActivityAt { get; init; }
}

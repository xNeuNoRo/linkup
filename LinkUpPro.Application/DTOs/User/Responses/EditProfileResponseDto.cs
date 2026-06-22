namespace LinkUpPro.Application.DTOs.User.Responses;

public record EditProfileResponseDto
{
    public string Id { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public bool HasError { get; init; }
    public List<string> Errors { get; init; } = [];
    public bool RequiresReLogin { get; init; }

    public static EditProfileResponseDto CreateSuccess(
        string id,
        string firstName,
        string lastName,
        string email,
        string userName,
        bool isVerified
    ) =>
        new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = userName,
            IsVerified = isVerified,
        };

    public static EditProfileResponseDto CreateSuccessWithReLogin(
        string id,
        string firstName,
        string lastName,
        string email,
        string userName,
        bool isVerified
    ) =>
        new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = userName,
            IsVerified = isVerified,
            RequiresReLogin = true,
        };

    public static EditProfileResponseDto CreateError(List<string> errors) =>
        new() { HasError = true, Errors = errors };
}

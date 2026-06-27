namespace LinkUpPro.Domain.Entities.Friendship;

public sealed record UserSearchResult(
    string Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string? ProfilePicturePath
);

namespace LinkUpPro.Domain.Common;

public sealed record UserSearchResult(
    string Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string? ProfilePicturePath
);

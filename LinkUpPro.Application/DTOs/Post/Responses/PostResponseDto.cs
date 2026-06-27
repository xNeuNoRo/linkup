namespace LinkUpPro.Application.DTOs.Post.Responses;

public record PostResponseDto(
    long Id,
    string AuthorId,
    string AuthorName,
    string? AuthorProfilePicture,
    string Content,
    int ContentType,
    string? MediaPath,
    int Privacy,
    bool AllowComments,
    bool IsEdited,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

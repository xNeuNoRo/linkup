using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Post.Responses;

public record PostResponseDto(
    long Id, string AuthorId, string AuthorName, string? AuthorProfilePicture,
    string Content, PostContentType ContentType, string? MediaPath, PrivacyLevel Privacy,
    bool AllowComments, bool IsEdited, DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

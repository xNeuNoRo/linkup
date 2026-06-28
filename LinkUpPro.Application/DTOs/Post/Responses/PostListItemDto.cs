using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Post.Responses;

public record PostListItemDto(
    long Id, string AuthorId, string AuthorName, string AuthorUserName,
    string? AuthorProfilePicture, string Content, PostContentType ContentType,
    string? MediaPath, PrivacyLevel Privacy, bool AllowComments, bool IsEdited,
    DateTimeOffset CreatedAt, int LikesCount, int DislikesCount, int CommentsCount);

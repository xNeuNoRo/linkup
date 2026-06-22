namespace LinkUpPro.Application.DTOs.Comment.Responses;

public record CommentResponseDto(
    long Id, long PostId, string AuthorId, string AuthorName,
    string? AuthorProfilePicture, string Content, bool IsEdited,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    long? ParentCommentId, int RepliesCount);

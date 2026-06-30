namespace LinkUpPro.Application.DTOs.Comment.Responses;

public record CommentTreeDto(
    CommentResponseDto Comment,
    List<CommentTreeDto> Replies,
    int TotalRepliesCount,
    bool HasMoreReplies,
    int CurrentRepliesPage,
    int RepliesPageSize,
    int VisualDepth = 0,
    bool IsTruncated = false,
    string? ReplyingToUserName = null,
    bool ShowConnector = false
);

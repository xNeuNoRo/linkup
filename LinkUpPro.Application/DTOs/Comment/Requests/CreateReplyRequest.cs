namespace LinkUpPro.Application.DTOs.Comment.Requests;

public record CreateReplyRequest(long ParentCommentId, string Content);

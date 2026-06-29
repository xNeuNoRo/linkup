namespace LinkUpPro.Application.DTOs.Comment.Requests;

public record CreateCommentRequest(long PostId, string Content);

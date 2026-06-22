namespace LinkUpPro.Application.DTOs.Comment.Responses;

public record CommentTreeDto(CommentResponseDto Comment, List<CommentTreeDto> Replies);

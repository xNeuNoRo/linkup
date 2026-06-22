using LinkUpPro.Application.DTOs.Comment.Requests;
using LinkUpPro.Application.DTOs.Comment.Responses;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Interfaces.Services;

public interface ICommentService
{
    Task<Result<CommentResponseDto>> CreateAsync(string authorId, CreateCommentRequest request);

    Task<Result<CommentResponseDto>> CreateReplyAsync(string authorId, CreateReplyRequest request);

    Task<Result<CommentResponseDto>> UpdateAsync(string authorId, long commentId, UpdateCommentRequest request);

    Task<Result> DeleteAsync(string authorId, long commentId);

    Task<List<CommentTreeDto>> GetPostCommentsAsync(string requesterId, long postId);
}

using LinkUpPro.Domain.Common;

namespace LinkUpPro.Domain.Entities.Social;

public sealed class Comment : AuditableBaseEntity<long>
{
    public const string DeletedCommentText = "Este comentario fue eliminado.";

    private Comment() { }

    public long PostId { get; private set; }

    public string AuthorId { get; private set; } = null!;

    public long? ParentCommentId { get; private set; }

    public string Content { get; private set; } = null!;

    public bool IsEdited { get; private set; }

    public static Result<Comment> Create(
        long postId,
        string authorId,
        string content,
        long? parentCommentId = null,
        DateTimeOffset? createdAt = null
    )
    {
        var errors = Validate(postId, authorId, content, parentCommentId);

        if (errors.Count > 0)
        {
            return Result<Comment>.Failure(errors);
        }

        return Result<Comment>.Success(
            new Comment
            {
                PostId = postId,
                AuthorId = authorId.Trim(),
                ParentCommentId = parentCommentId,
                Content = content.Trim(),
                IsEdited = false,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            }
        );
    }

    public Result Edit(string content, DateTimeOffset? updatedAt = null)
    {
        var errors = Validate(PostId, AuthorId, content, ParentCommentId);

        if (errors.Count > 0)
        {
            return Result.Failure(errors);
        }

        Content = content.Trim();
        IsEdited = true;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void MarkAsDeleted(bool hasReplies, DateTimeOffset? deletedAt = null)
    {
        if (hasReplies)
        {
            Content = DeletedCommentText;
        }

        base.MarkAsDeleted(deletedAt);
    }

    public bool IsRootComment() => ParentCommentId is null;

    public bool CanBeEditedBy(string userId) => !IsDeleted && AuthorId == userId;

    private static List<DomainError> Validate(
        long postId,
        string authorId,
        string content,
        long? parentCommentId
    )
    {
        var errors = new List<DomainError>();

        if (postId <= 0)
        {
            errors.Add(
                new DomainError("Comment.InvalidPost", "La publicacion relacionada no es valida.")
            );
        }

        if (string.IsNullOrWhiteSpace(authorId))
        {
            errors.Add(
                new DomainError("Comment.AuthorRequired", "El autor del comentario es requerido.")
            );
        }

        if (parentCommentId <= 0)
        {
            errors.Add(
                new DomainError("Comment.InvalidParent", "El comentario padre no es valido.")
            );
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            errors.Add(
                new DomainError(
                    "Comment.ContentRequired",
                    "Debe ingresar el contenido del comentario."
                )
            );
        }
        else if (content.Trim().Length > DomainConstants.MaxCommentContentLength)
        {
            errors.Add(
                new DomainError(
                    "Comment.ContentTooLong",
                    "El comentario no puede superar los 500 caracteres."
                )
            );
        }

        return errors;
    }
}

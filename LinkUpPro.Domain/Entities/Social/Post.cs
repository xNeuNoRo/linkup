using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.Entities.Social;

public sealed class Post : AuditableBaseEntity<long>
{
    private Post()
    {
    }

    public string AuthorId { get; private set; } = null!;

    public string Content { get; private set; } = null!;

    public PostContentType ContentType { get; private set; }

    public string MediaPath { get; private set; } = null!;

    public PrivacyLevel Privacy { get; private set; }

    public bool AllowComments { get; private set; }

    public bool IsEdited { get; private set; }

    public static Result<Post> Create(
        string authorId,
        string content,
        PostContentType contentType,
        string mediaPath,
        PrivacyLevel privacy = PrivacyLevel.FriendsOnly,
        bool allowComments = true,
        DateTimeOffset? createdAt = null)
    {
        var errors = Validate(authorId, content, contentType, mediaPath, privacy);

        if (errors.Count > 0)
        {
            return Result<Post>.Failure(errors);
        }

        return Result<Post>.Success(new Post
        {
            AuthorId = authorId.Trim(),
            Content = content.Trim(),
            ContentType = contentType,
            MediaPath = mediaPath.Trim(),
            Privacy = privacy,
            AllowComments = allowComments,
            IsEdited = false,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
        });
    }

    public Result Edit(
        string content,
        PostContentType contentType,
        string mediaPath,
        PrivacyLevel privacy,
        bool allowComments,
        DateTimeOffset? updatedAt = null)
    {
        var errors = Validate(AuthorId, content, contentType, mediaPath, privacy);

        if (errors.Count > 0)
        {
            return Result.Failure(errors);
        }

        Content = content.Trim();
        ContentType = contentType;
        MediaPath = mediaPath.Trim();
        Privacy = privacy;
        AllowComments = allowComments;
        IsEdited = true;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public bool CanBeViewedBy(string userId, bool isFriend)
    {
        if (IsDeleted || string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        if (AuthorId == userId)
        {
            return true;
        }

        return Privacy == PrivacyLevel.FriendsOnly && isFriend;
    }

    public bool CanBeEditedBy(string userId) => !IsDeleted && AuthorId == userId;

    public bool CanComment(string userId, bool isFriend) => AllowComments && CanBeViewedBy(userId, isFriend);

    private static List<DomainError> Validate(
        string authorId,
        string content,
        PostContentType contentType,
        string mediaPath,
        PrivacyLevel privacy)
    {
        var errors = new List<DomainError>();

        if (string.IsNullOrWhiteSpace(authorId))
        {
            errors.Add(new DomainError("Post.AuthorRequired", "El autor de la publicacion es requerido."));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            errors.Add(new DomainError("Post.ContentRequired", "Debe ingresar el contenido de la publicacion."));
        }
        else if (content.Trim().Length > DomainConstants.MaxPostContentLength)
        {
            errors.Add(new DomainError("Post.ContentTooLong", "El contenido no puede superar los 1,000 caracteres."));
        }

        if (!Enum.IsDefined(contentType))
        {
            errors.Add(new DomainError("Post.InvalidContentType", "El tipo de contenido seleccionado no es valido."));
        }

        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            errors.Add(new DomainError("Post.MediaRequired", "Debe seleccionar una imagen o ingresar un enlace valido de YouTube."));
        }

        if (!Enum.IsDefined(privacy))
        {
            errors.Add(new DomainError("Post.InvalidPrivacy", "La privacidad seleccionada no es valida."));
        }

        return errors;
    }
}

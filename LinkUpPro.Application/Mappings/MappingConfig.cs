using LinkUpPro.Application.DTOs.Battleship.Responses;
using LinkUpPro.Application.DTOs.Comment.Responses;
using LinkUpPro.Application.DTOs.FriendRequest.Responses;
using LinkUpPro.Application.DTOs.Friendship.Responses;
using LinkUpPro.Application.DTOs.Notification.Responses;
using LinkUpPro.Application.DTOs.Post.Responses;
using LinkUpPro.Application.DTOs.Reaction.Responses;
using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Entities.Social;
using Mapster;

namespace LinkUpPro.Application.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // ==========================================
        // Post
        // ==========================================
        TypeAdapterConfig<Post, PostResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.AuthorId, src => src.AuthorId)
            .Map(dest => dest.Content, src => src.Content)
            .Map(dest => dest.ContentType, src => (int)src.ContentType)
            .Map(dest => dest.MediaPath, src => src.MediaPath)
            .Map(dest => dest.Privacy, src => (int)src.Privacy)
            .Map(dest => dest.AllowComments, src => src.AllowComments)
            .Map(dest => dest.IsEdited, src => src.IsEdited)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
        // AuthorName, AuthorProfilePicture, AuthorUserName - se setean manualmente en service

        TypeAdapterConfig<Post, PostListItemDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.AuthorId, src => src.AuthorId)
            .Map(dest => dest.Content, src => src.Content)
            .Map(dest => dest.ContentType, src => (int)src.ContentType)
            .Map(dest => dest.MediaPath, src => src.MediaPath)
            .Map(dest => dest.Privacy, src => (int)src.Privacy)
            .Map(dest => dest.AllowComments, src => src.AllowComments)
            .Map(dest => dest.IsEdited, src => src.IsEdited)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // ==========================================
        // Comment
        // ==========================================
        TypeAdapterConfig<Comment, CommentResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PostId, src => src.PostId)
            .Map(dest => dest.AuthorId, src => src.AuthorId)
            .Map(dest => dest.Content, src => src.Content)
            .Map(dest => dest.IsEdited, src => src.IsEdited)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
            .Map(dest => dest.ParentCommentId, src => src.ParentCommentId);
        // AuthorName, AuthorProfilePicture, RepliesCount - se setean manualmente en service

        // ==========================================
        // Reaction
        // ==========================================
        TypeAdapterConfig<Reaction, ReactionResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PostId, src => src.PostId)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Type, src => (int)src.Type)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // ==========================================
        // Friendship
        // ==========================================
        TypeAdapterConfig<Friendship, FriendshipResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.IsActive, src => !src.IsDeleted);
        // UserId, FriendId - se setean manualmente en service

        TypeAdapterConfig<Friendship, FriendListItemDto>.NewConfig();
        // FriendId, FriendName, FriendUserName, FriendProfilePicturePath, CommonFriendsCount - manual

        // ==========================================
        // FriendRequest
        // ==========================================
        TypeAdapterConfig<FriendRequest, PendingRequestDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SenderId, src => src.SenderId)
            .Map(dest => dest.SentAt, src => src.SentAt);
        // SenderName, SenderUserName, SenderProfilePicture, CommonFriendsCount - manual

        TypeAdapterConfig<FriendRequest, SentRequestDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ReceiverId, src => src.ReceiverId)
            .Map(dest => dest.SentAt, src => src.SentAt)
            .Map(dest => dest.Status, src => (int)src.Status)
            .Map(dest => dest.RespondedAt, src => src.RespondedAt)
            .Map(dest => dest.IsVisibleForSender, src => src.IsVisibleForSender);
        // ReceiverName, ReceiverUserName, ReceiverProfilePicture - manual

        // ==========================================
        // Notification
        // ==========================================
        TypeAdapterConfig<Notification, NotificationResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ActorId, src => src.ActorId)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Message, src => src.Message)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.IsRead, src => src.IsRead)
            .Map(dest => dest.RelatedEntityId, src => src.RelatedEntityId);
        // ActorName, ActorProfilePicture - manual

        // ==========================================
        // Battleship
        // ==========================================
        TypeAdapterConfig<BattleshipGame, GameResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CreatorId, src => src.CreatorId)
            .Map(dest => dest.OpponentId, src => src.OpponentId)
            .Map(dest => dest.Status, src => (int)src.Status)
            .Map(dest => dest.CurrentTurnUserId, src => src.CurrentTurnUserId)
            .Map(dest => dest.WinnerId, src => src.WinnerId)
            .Map(dest => dest.StartedAt, src => src.StartedAt)
            .Map(dest => dest.FinishedAt, src => src.FinishedAt);
        // CreatorName, OpponentName - manual

        TypeAdapterConfig<BattleshipGame, GameListItemDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Status, src => (int)src.Status)
            .Map(dest => dest.StartedAt, src => src.StartedAt)
            .Map(dest => dest.FinishedAt, src => src.FinishedAt)
            .Map(dest => dest.WinnerId, src => src.WinnerId)
            .Map(dest => dest.CurrentTurnUserId, src => src.CurrentTurnUserId);
        // OpponentId, OpponentName, Duration - manual

        TypeAdapterConfig<BattleshipGame, GameHistoryDto>
            .NewConfig()
            .Map(dest => dest.GameId, src => src.Id)
            .Map(dest => dest.StartedAt, src => src.StartedAt)
            .Map(dest => dest.FinishedAt, src => src.FinishedAt ?? DateTimeOffset.UtcNow);
        // OpponentId, OpponentName, IsWon, Winner, OpponentProfilePicture - manual
    }
}

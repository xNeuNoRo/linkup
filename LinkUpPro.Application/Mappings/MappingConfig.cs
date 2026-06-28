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
        // Friendship
        TypeAdapterConfig<Friendship, FriendshipResponseDto>
            .NewConfig()
            .Map(dest => dest.IsActive, src => !src.IsDeleted);

        // BattleshipGame
        TypeAdapterConfig<BattleshipGame, GameHistoryDto>
            .NewConfig()
            .Map(dest => dest.GameId, src => src.Id)
            .Map(dest => dest.StartedAt, src => src.StartedAt)
            .Map(dest => dest.FinishedAt, src => src.FinishedAt ?? DateTimeOffset.UtcNow);
    }
}

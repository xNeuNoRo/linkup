namespace LinkUpPro.Application.DTOs.Post.Responses;

public record PostStatsDto(
    int TotalPosts, int ImagePosts, int VideoPosts,
    int FriendsOnlyPosts, int OnlyMePosts, int EditedPosts);

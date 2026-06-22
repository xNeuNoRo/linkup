namespace LinkUpPro.Application.DTOs.Post.Requests;

public record CreatePostRequest(
    string Content, int ContentType, string? MediaPath, string? YouTubeUrl,
    int Privacy, bool AllowComments);

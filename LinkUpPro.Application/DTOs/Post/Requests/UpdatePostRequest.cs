namespace LinkUpPro.Application.DTOs.Post.Requests;

public record UpdatePostRequest(
    string? Content, int? ContentType, string? MediaPath,
    string? YouTubeUrl, int? Privacy, bool? AllowComments);

using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.DTOs.Post.Requests;

public record UpdatePostRequest(
    string? Content,
    int? ContentType,
    IFormFile? ImageFile,
    string? YouTubeUrl,
    int? Privacy,
    bool? AllowComments
);

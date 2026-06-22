namespace LinkUpPro.Application.DTOs.Post.Requests;

public record PostFilterRequest(
    string? SearchText, int? ContentType, DateTimeOffset? FromDate,
    DateTimeOffset? ToDate, bool? EditedOnly, int Page = 1, int PageSize = 20);

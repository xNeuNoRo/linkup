namespace LinkUpPro.Application.DTOs.FriendRequest.Responses;

public record SentRequestDto(
    long Id, string ReceiverId, string ReceiverName, string ReceiverUserName,
    string? ReceiverProfilePicture, DateTimeOffset SentAt, int Status,
    DateTimeOffset? RespondedAt, bool IsVisibleForSender);

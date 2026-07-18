using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.FriendRequest.Responses;

public record SentRequestDto(
    long Id,
    string ReceiverId,
    string ReceiverName,
    string ReceiverUserName,
    string? ReceiverProfilePicture,
    int CommonFriendsCount,
    DateTimeOffset SentAt,
    FriendRequestStatus Status,
    DateTimeOffset? RespondedAt,
    bool IsVisibleForSender,
    string SenderName
);

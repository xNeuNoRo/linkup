using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Entities.Friendship;

public class FriendRequestTests
{
    [Fact]
    public void Create_ValidUsers_CreatesPendingVisibleRequest()
    {
        // Arrange & Act
        var result = FriendRequest.Create("sender", "receiver");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FriendRequestStatus.Pending, result.Value.Status);
        Assert.True(result.Value.IsVisibleForSender);
        Assert.True(result.Value.CanBeAcceptedBy("receiver"));
    }

    [Fact]
    public void Create_SameUser_ReturnsFailure()
    {
        // Arrange & Act
        var result = FriendRequest.Create("user", "user");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "FriendRequest.SelfRequestNotAllowed");
    }

    [Fact]
    public void Accept_ByReceiver_ChangesStatusToAccepted()
    {
        // Arrange
        var request = FriendRequest.Create("sender", "receiver").Value;

        // Act
        var result = request.Accept("receiver");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FriendRequestStatus.Accepted, request.Status);
        Assert.NotNull(request.RespondedAt);
    }

    [Fact]
    public void Accept_BySender_ReturnsFailure()
    {
        // Arrange
        var request = FriendRequest.Create("sender", "receiver").Value;

        // Act
        var result = request.Accept("sender");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FriendRequest.AcceptNotAllowed", result.Error.Code);
    }

    [Fact]
    public void Reject_AfterAccepted_ReturnsNotPendingFailure()
    {
        // Arrange
        var request = FriendRequest.Create("sender", "receiver").Value;
        request.Accept("receiver");

        // Act
        var result = request.Reject("receiver");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FriendRequest.NotPending", result.Error.Code);
    }

    [Fact]
    public void Cancel_BySender_ChangesStatusToCanceled()
    {
        // Arrange
        var request = FriendRequest.Create("sender", "receiver").Value;

        // Act
        var result = request.Cancel("sender");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FriendRequestStatus.Canceled, request.Status);
    }

    [Fact]
    public void HideFromSenderHistory_AcceptedRequest_HidesOnlyForSender()
    {
        // Arrange
        var request = FriendRequest.Create("sender", "receiver").Value;
        request.Accept("receiver");

        // Act
        var result = request.HideFromSenderHistory("sender");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(request.IsVisibleForSender);
    }

    [Fact]
    public void HideFromSenderHistory_PendingRequest_ReturnsFailure()
    {
        // Arrange
        var request = FriendRequest.Create("sender", "receiver").Value;

        // Act
        var result = request.HideFromSenderHistory("sender");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FriendRequest.InvalidStatusForHistoryHide", result.Error.Code);
    }
}

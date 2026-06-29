using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Enums;

public class FriendRequestStatusTests
{
    [Fact]
    public void FriendRequestStatus_Values_MatchDatabaseContract()
    {
        // Arrange & Act & Assert
        Assert.Equal(1, (int)FriendRequestStatus.Pending);
        Assert.Equal(2, (int)FriendRequestStatus.Accepted);
        Assert.Equal(3, (int)FriendRequestStatus.Rejected);
        Assert.Equal(4, (int)FriendRequestStatus.Canceled);
    }
}

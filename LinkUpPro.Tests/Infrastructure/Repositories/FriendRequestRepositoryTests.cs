using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class FriendRequestRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetPendingReceivedAsync_ReturnsPending()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var senderId = CreateUserId("sender");
        var receiverId = CreateUserId("receiver");

        context.FriendRequests.Add(FriendRequest.Create(senderId, receiverId).Value);
        await context.SaveChangesAsync();

        var result = await repo.GetPendingReceivedAsync(receiverId);

        result.Should().HaveCount(1);
        result.First().SenderId.Should().Be(senderId);
    }

    [Fact]
    public async Task GetPendingSentAsync_ReturnsPending()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var senderId = CreateUserId("sender");
        var receiverId = CreateUserId("receiver");

        context.FriendRequests.Add(FriendRequest.Create(senderId, receiverId).Value);
        await context.SaveChangesAsync();

        var result = await repo.GetPendingSentAsync(senderId);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetVisibleSentHistoryAsync_ExcludesPendingAndCanceled()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var senderId = CreateUserId("sender");
        var receiverId = CreateUserId("receiver");

        var pending = FriendRequest.Create(senderId, receiverId).Value;
        context.FriendRequests.Add(pending);
        await context.SaveChangesAsync();

        pending.Accept(receiverId);
        await context.SaveChangesAsync();

        var result = await repo.GetVisibleSentHistoryAsync(senderId);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(FriendRequestStatus.Accepted);
    }

    [Fact]
    public async Task ExistsPendingBetweenAsync_Bidirectional_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        context.FriendRequests.Add(FriendRequest.Create(userAId, userBId).Value);
        await context.SaveChangesAsync();

        var existsAB = await repo.ExistsPendingBetweenAsync(userAId, userBId);
        var existsBA = await repo.ExistsPendingBetweenAsync(userBId, userAId);

        existsAB.Should().BeTrue();
        existsBA.Should().BeTrue();
    }

    [Fact]
    public async Task GetPendingBetweenAsync_ReturnsCorrectRequest()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        var fr = FriendRequest.Create(userAId, userBId).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        var result = await repo.GetPendingBetweenAsync(userAId, userBId);

        result.Should().NotBeNull();
        result!.SenderId.Should().Be(userAId);
        result.ReceiverId.Should().Be(userBId);
    }

    [Fact]
    public async Task GetByIdForSenderAsync_ReturnsOnlyIfSender()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var senderId = CreateUserId("sender");
        var receiverId = CreateUserId("receiver");
        var otherId = CreateUserId("other");

        var fr = FriendRequest.Create(senderId, receiverId).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        var forSender = await repo.GetByIdForSenderAsync(fr.Id, senderId);
        var forOther = await repo.GetByIdForSenderAsync(fr.Id, otherId);

        forSender.Should().NotBeNull();
        forOther.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdForReceiverAsync_ReturnsOnlyIfReceiver()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var senderId = CreateUserId("sender");
        var receiverId = CreateUserId("receiver");

        var fr = FriendRequest.Create(senderId, receiverId).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        var forReceiver = await repo.GetByIdForReceiverAsync(fr.Id, receiverId);

        forReceiver.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsPendingBetweenAsync_NonPending_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        var fr = FriendRequest.Create(userAId, userBId).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        fr.Reject(userBId);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsPendingBetweenAsync(userAId, userBId);

        exists.Should().BeFalse();
    }
}

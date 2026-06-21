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
        var sender = await SeedUserAsync(context, "sender", "s@t.com");
        var receiver = await SeedUserAsync(context, "receiver", "r@t.com");

        context.FriendRequests.Add(FriendRequest.Create(sender.Id, receiver.Id).Value);
        await context.SaveChangesAsync();

        var result = await repo.GetPendingReceivedAsync(receiver.Id);

        result.Should().HaveCount(1);
        result.First().SenderId.Should().Be(sender.Id);
    }

    [Fact]
    public async Task GetPendingSentAsync_ReturnsPending()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var sender = await SeedUserAsync(context, "sender", "s@t.com");
        var receiver = await SeedUserAsync(context, "receiver", "r@t.com");

        context.FriendRequests.Add(FriendRequest.Create(sender.Id, receiver.Id).Value);
        await context.SaveChangesAsync();

        var result = await repo.GetPendingSentAsync(sender.Id);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetVisibleSentHistoryAsync_ExcludesPendingAndCanceled()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var sender = await SeedUserAsync(context, "sender", "s@t.com");
        var receiver = await SeedUserAsync(context, "receiver", "r@t.com");

        var pending = FriendRequest.Create(sender.Id, receiver.Id).Value;
        context.FriendRequests.Add(pending);
        await context.SaveChangesAsync();

        pending.Accept(receiver.Id);
        await context.SaveChangesAsync();

        var result = await repo.GetVisibleSentHistoryAsync(sender.Id);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(FriendRequestStatus.Accepted);
    }

    [Fact]
    public async Task ExistsPendingBetweenAsync_Bidirectional_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        context.FriendRequests.Add(FriendRequest.Create(userA.Id, userB.Id).Value);
        await context.SaveChangesAsync();

        var existsAB = await repo.ExistsPendingBetweenAsync(userA.Id, userB.Id);
        var existsBA = await repo.ExistsPendingBetweenAsync(userB.Id, userA.Id);

        existsAB.Should().BeTrue();
        existsBA.Should().BeTrue();
    }

    [Fact]
    public async Task GetPendingBetweenAsync_ReturnsCorrectRequest()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var fr = FriendRequest.Create(userA.Id, userB.Id).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        var result = await repo.GetPendingBetweenAsync(userA.Id, userB.Id);

        result.Should().NotBeNull();
        result!.SenderId.Should().Be(userA.Id);
        result.ReceiverId.Should().Be(userB.Id);
    }

    [Fact]
    public async Task GetByIdForSenderAsync_ReturnsOnlyIfSender()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var sender = await SeedUserAsync(context, "sender", "s@t.com");
        var receiver = await SeedUserAsync(context, "receiver", "r@t.com");
        var other = await SeedUserAsync(context, "other", "o@t.com");

        var fr = FriendRequest.Create(sender.Id, receiver.Id).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        var forSender = await repo.GetByIdForSenderAsync(fr.Id, sender.Id);
        var forOther = await repo.GetByIdForSenderAsync(fr.Id, other.Id);

        forSender.Should().NotBeNull();
        forOther.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdForReceiverAsync_ReturnsOnlyIfReceiver()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var sender = await SeedUserAsync(context, "sender", "s@t.com");
        var receiver = await SeedUserAsync(context, "receiver", "r@t.com");

        var fr = FriendRequest.Create(sender.Id, receiver.Id).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        var forReceiver = await repo.GetByIdForReceiverAsync(fr.Id, receiver.Id);

        forReceiver.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsPendingBetweenAsync_NonPending_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new FriendRequestRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var fr = FriendRequest.Create(userA.Id, userB.Id).Value;
        context.FriendRequests.Add(fr);
        await context.SaveChangesAsync();

        fr.Reject(userB.Id);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsPendingBetweenAsync(userA.Id, userB.Id);

        exists.Should().BeFalse();
    }
}

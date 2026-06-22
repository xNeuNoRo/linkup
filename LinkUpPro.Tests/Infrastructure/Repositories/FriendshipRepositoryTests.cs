using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class FriendshipRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task AreFriendsAsync_ActiveFriendship_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        context.Friendships.Add(Friendship.Create(userAId, userBId).Value);
        await context.SaveChangesAsync();

        var areFriends = await repo.AreFriendsAsync(userAId, userBId);

        areFriends.Should().BeTrue();
    }

    [Fact]
    public async Task AreFriendsAsync_NoFriendship_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        var areFriends = await repo.AreFriendsAsync(userAId, userBId);

        areFriends.Should().BeFalse();
    }

    [Fact]
    public async Task AreFriendsAsync_DeletedFriendship_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        var friendship = Friendship.Create(userAId, userBId).Value;
        context.Friendships.Add(friendship);
        await context.SaveChangesAsync();
        friendship.MarkAsDeleted();
        await context.SaveChangesAsync();

        var areFriends = await repo.AreFriendsAsync(userAId, userBId);

        areFriends.Should().BeFalse();
    }

    [Fact]
    public async Task GetFriendshipBetweenAsync_WithIgnoreFilters_IncludesDeleted()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        var friendship = Friendship.Create(userAId, userBId).Value;
        context.Friendships.Add(friendship);
        await context.SaveChangesAsync();
        friendship.MarkAsDeleted();
        await context.SaveChangesAsync();

        var result = await repo.GetFriendshipBetweenAsync(userAId, userBId);

        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveFriendIdsAsync_ReturnsFriendIds()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");
        var userCId = CreateUserId("userC");

        context.Friendships.AddRange(
            Friendship.Create(userAId, userBId).Value,
            Friendship.Create(userAId, userCId).Value
        );
        await context.SaveChangesAsync();

        var friendIds = await repo.GetActiveFriendIdsAsync(userAId);

        friendIds.Should().HaveCount(2);
        friendIds.Should().Contain(userBId);
        friendIds.Should().Contain(userCId);
    }

    [Fact]
    public async Task GetActiveFriendsCountAsync_ReturnsCount()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        context.Friendships.Add(Friendship.Create(userAId, userBId).Value);
        await context.SaveChangesAsync();

        var count = await repo.GetActiveFriendsCountAsync(userAId);

        count.Should().Be(1);
    }

    [Fact]
    public async Task GetCommonFriendsCountAsync_ReturnsCommonCount()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");
        var commonId = CreateUserId("common");

        context.Friendships.AddRange(
            Friendship.Create(userAId, commonId).Value,
            Friendship.Create(userBId, commonId).Value
        );
        await context.SaveChangesAsync();

        var count = await repo.GetCommonFriendsCountAsync(userAId, userBId);

        count.Should().Be(1);
    }

    [Fact]
    public async Task GetActiveFriendshipsForUserAsync_ReturnsActive()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userAId = CreateUserId("userA");
        var userBId = CreateUserId("userB");

        var f = Friendship.Create(userAId, userBId).Value;
        context.Friendships.Add(f);
        await context.SaveChangesAsync();

        var result = await repo.GetActiveFriendshipsForUserAsync(userAId);

        result.Should().HaveCount(1);
    }
}

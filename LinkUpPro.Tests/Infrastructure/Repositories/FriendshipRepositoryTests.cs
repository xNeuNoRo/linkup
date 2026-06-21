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
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        context.Friendships.Add(Friendship.Create(userA.Id, userB.Id).Value);
        await context.SaveChangesAsync();

        var areFriends = await repo.AreFriendsAsync(userA.Id, userB.Id);

        areFriends.Should().BeTrue();
    }

    [Fact]
    public async Task AreFriendsAsync_NoFriendship_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var areFriends = await repo.AreFriendsAsync(userA.Id, userB.Id);

        areFriends.Should().BeFalse();
    }

    [Fact]
    public async Task AreFriendsAsync_DeletedFriendship_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var friendship = Friendship.Create(userA.Id, userB.Id).Value;
        context.Friendships.Add(friendship);
        await context.SaveChangesAsync();
        friendship.MarkAsDeleted();
        await context.SaveChangesAsync();

        var areFriends = await repo.AreFriendsAsync(userA.Id, userB.Id);

        areFriends.Should().BeFalse();
    }

    [Fact]
    public async Task GetFriendshipBetweenAsync_WithIgnoreFilters_IncludesDeleted()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var friendship = Friendship.Create(userA.Id, userB.Id).Value;
        context.Friendships.Add(friendship);
        await context.SaveChangesAsync();
        friendship.MarkAsDeleted();
        await context.SaveChangesAsync();

        var result = await repo.GetFriendshipBetweenAsync(userA.Id, userB.Id);

        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveFriendIdsAsync_ReturnsFriendIds()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");
        var userC = await SeedUserAsync(context, "userC", "c@t.com");

        context.Friendships.AddRange(
            Friendship.Create(userA.Id, userB.Id).Value,
            Friendship.Create(userA.Id, userC.Id).Value
        );
        await context.SaveChangesAsync();

        var friendIds = await repo.GetActiveFriendIdsAsync(userA.Id);

        friendIds.Should().HaveCount(2);
        friendIds.Should().Contain(userB.Id);
        friendIds.Should().Contain(userC.Id);
    }

    [Fact]
    public async Task GetActiveFriendsCountAsync_ReturnsCount()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        context.Friendships.Add(Friendship.Create(userA.Id, userB.Id).Value);
        await context.SaveChangesAsync();

        var count = await repo.GetActiveFriendsCountAsync(userA.Id);

        count.Should().Be(1);
    }

    [Fact]
    public async Task GetCommonFriendsCountAsync_ReturnsCommonCount()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");
        var common = await SeedUserAsync(context, "common", "common@t.com");

        context.Friendships.AddRange(
            Friendship.Create(userA.Id, common.Id).Value,
            Friendship.Create(userB.Id, common.Id).Value
        );
        await context.SaveChangesAsync();

        var count = await repo.GetCommonFriendsCountAsync(userA.Id, userB.Id);

        count.Should().Be(1);
    }

    [Fact]
    public async Task GetActiveFriendshipsForUserAsync_ReturnsActive()
    {
        var context = CreateContext();
        var repo = new FriendshipRepository(context);
        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var f = Friendship.Create(userA.Id, userB.Id).Value;
        context.Friendships.Add(f);
        await context.SaveChangesAsync();

        var result = await repo.GetActiveFriendshipsForUserAsync(userA.Id);

        result.Should().HaveCount(1);
    }
}

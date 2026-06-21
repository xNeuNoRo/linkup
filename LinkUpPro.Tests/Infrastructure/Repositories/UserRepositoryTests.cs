using LinkUpPro.Domain.ValueObjects;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class UserRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user = await SeedUserAsync(context, "testuser", "findme@linkuppro.com");

        var result = await repo.GetByEmailAsync(Email.Create("findme@linkuppro.com"));

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var result = await repo.GetByEmailAsync(Email.Create("nobody@linkuppro.com"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserNameAsync_ExistingUserName_ReturnsUser()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user = await SeedUserAsync(context, "findme");

        var result = await repo.GetByUserNameAsync("findme");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task ExistsByEmailAsync_DuplicateEmail_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        await SeedUserAsync(context, "user1", "dup@linkuppro.com");

        var exists = await repo.ExistsByEmailAsync(Email.Create("dup@linkuppro.com"));

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ExcludeUserId_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user = await SeedUserAsync(context, "user1", "dup@linkuppro.com");

        var exists = await repo.ExistsByEmailAsync(
            Email.Create("dup@linkuppro.com"),
            excludeUserId: user.Id
        );

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task IsActiveAsync_ActiveUser_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user = await SeedUserAsync(context);

        var isActive = await repo.IsActiveAsync(user.Id);

        isActive.Should().BeTrue();
    }

    [Fact]
    public async Task IsActiveAsync_DeletedUser_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var user = await SeedUserAsync(context);
        user.MarkAsDeleted();
        await context.SaveChangesAsync();

        var isActive = await repo.IsActiveAsync(user.Id);

        isActive.Should().BeFalse();
    }

    [Fact]
    public async Task SearchActiveUsersAsync_ByUserName_ReturnsMatches()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        await SeedUserAsync(context, "john_doe", "john@test.com");
        await SeedUserAsync(context, "jane_doe", "jane@test.com");
        await SeedUserAsync(context, "admin", "admin@test.com");

        var result = await repo.SearchActiveUsersAsync("john");

        result.Should().HaveCount(1);
        result.First().UserName.Should().Be("john_doe");
    }

    [Fact]
    public async Task SearchActiveUsersAsync_EmptySearch_ReturnsAllActive()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        await SeedUserAsync(context, "user1", "u1@t.com");
        await SeedUserAsync(context, "user2", "u2@t.com");

        var result = await repo.SearchActiveUsersAsync(null);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveUsersAsync_ReturnsOnlyActiveUsers()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);
        var active = await SeedUserAsync(context, "active1", "active1@t.com");
        var inactive = CreateTestUser("inactive", "inactive@t.com");
        inactive.DeactivateAccount();
        context.Users.Add(inactive);
        await context.SaveChangesAsync();

        var result = await repo.GetActiveUsersAsync();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(active.Id);
    }

    [Fact]
    public async Task ExistsByUserNameAsync_NonExisting_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var exists = await repo.ExistsByUserNameAsync("nobody");

        exists.Should().BeFalse();
    }
}

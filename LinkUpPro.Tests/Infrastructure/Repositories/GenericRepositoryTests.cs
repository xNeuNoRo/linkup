using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.ValueObjects;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class GenericRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetAllAsync_NoOptions_ReturnsAllEntities()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var user1 = CreateTestUser("user1", "user1@test.com");
        var user2 = CreateTestUser("user2", "user2@test.com");
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var result = await repo.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var user = CreateTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var result = await repo.GetByIdAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ValidEntity_PersistsAndCanBeRetrieved()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var user = CreateTestUser();
        await repo.AddAsync(user);
        await context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(user.Id);
        retrieved.Should().NotBeNull();
        retrieved!.UserName.Should().Be("testuser");
    }

    [Fact]
    public async Task AddRangeAsync_MultipleEntities_AllPersisted()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var users = new[]
        {
            CreateTestUser("user1", "user1@test.com"),
            CreateTestUser("user2", "user2@test.com"),
            CreateTestUser("user3", "user3@test.com"),
        };

        await repo.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        all.Should().HaveCount(3);
    }

    [Fact]
    public async Task Update_ExistingEntity_UpdatesProperties()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var user = CreateTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = user.UpdateProfile("Updated", "Name", PhoneNumber.Create("809-555-5678"));
        repo.Update(user);
        await context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(user.Id);
        retrieved!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task ExistsAsync_ExistingEntity_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var user = CreateTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsAsync(u => u.Id == user.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        var exists = await repo.ExistsAsync(u => u.Id == "nonexistent");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_NoPredicate_ReturnsTotalCount()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        context.Users.AddRange(CreateTestUser("u1", "u1@t.com"), CreateTestUser("u2", "u2@t.com"));
        await context.SaveChangesAsync();

        var count = await repo.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithQueryOptions_FilterWorks()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        context.Users.AddRange(
            CreateTestUser("alpha", "a@t.com"),
            CreateTestUser("beta", "b@t.com")
        );
        await context.SaveChangesAsync();

        var options = new QueryOptions<User> { Filter = u => u.UserName == "alpha" };
        var result = await repo.GetAllAsync(options);

        result.Should().HaveCount(1);
        result.First().UserName.Should().Be("alpha");
    }

    [Fact]
    public async Task GetAllAsync_WithQueryOptions_SkipTakeWorks()
    {
        var context = CreateContext();
        var repo = new UserRepository(context);

        context.Users.AddRange(
            CreateTestUser("user1", "u1@t.com"),
            CreateTestUser("user2", "u2@t.com"),
            CreateTestUser("user3", "u3@t.com")
        );
        await context.SaveChangesAsync();

        var options = new QueryOptions<User>
        {
            OrderBy = q => q.OrderBy(u => u.UserName),
            Skip = 1,
            Take = 1,
        };

        var result = await repo.GetAllAsync(options);

        result.Should().HaveCount(1);
        result.First().UserName.Should().Be("user2");
    }
}
